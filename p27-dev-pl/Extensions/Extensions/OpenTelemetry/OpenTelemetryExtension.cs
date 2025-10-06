using MassTransit.Logging;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using OpenTelemetry.Instrumentation.AspNetCore;
using OpenTelemetry.Logs;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using System.Diagnostics;
using Foundation.Settings;

namespace Extensions.OpenTelemetry
{
    public static class OpenTelemetryExtension
    {
        public static ActivitySource ActivitySource;
        public static IServiceCollection AddOpenTelemetry(this IServiceCollection services, AppSettings appSettings)
        {
            void ConfigureResource(ResourceBuilder r) => r
                .AddService(serviceName: appSettings?.DistributedTracing?.Jaeger?.ServiceName ?? string.Empty,
                serviceVersion: typeof(OpenTelemetryExtension).Assembly.GetName().Version?.ToString() ?? "unknown",
                serviceInstanceId: Environment.MachineName)
                .AddTelemetrySdk()
                .AddEnvironmentVariableDetector()
                .AddAttributes(new Dictionary<string, object>
                {
                    ["environment.name"] = "development",
                    ["team.name"] = "backend"
                });

            services.AddOpenTelemetry()
                .ConfigureResource(ConfigureResource)
                .WithTracing(p =>
                {
                    p.AddSource(DiagnosticHeaders.DefaultListenerName, appSettings.DistributedTracing.Jaeger.ServiceName, "MassTransit")
                        .AddAspNetCoreInstrumentation(p =>
                        {
                            p.RecordException = true;
                        })
                        .AddHttpClientInstrumentation(p =>
                        {
                            p.RecordException = true;
                        })
                        .AddSqlClientInstrumentation(p =>
                        {
                            p.SetDbStatementForText = true;
                            p.EnableConnectionLevelAttributes = true;
                            p.RecordException = true;
                        })
                        .AddEntityFrameworkCoreInstrumentation(p =>
                        {
                            p.SetDbStatementForText = true;
                        })
                        .AddMongoDBInstrumentation()
                        .AddMassTransitInstrumentation()
                        .SetSampler(new AlwaysOnSampler())
                        .AddJaegerExporter(p =>
                        {
                            p.AgentHost = appSettings?.DistributedTracing?.Jaeger?.Host;
                            p.AgentPort = appSettings?.DistributedTracing?.Jaeger?.Port ?? 0;
                        });
                });

            services.AddLogging(build =>
            {
                build.SetMinimumLevel(LogLevel.Debug);
                build.AddOpenTelemetry(options =>
                {
                    options.AddConsoleExporter().SetResourceBuilder(ResourceBuilder.CreateDefault()
                        .AddService(appSettings?.DistributedTracing?.Jaeger?.ServiceName ?? string.Empty))
                        .AddProcessor(new ActivityEventExtensions()).IncludeScopes = true;
                });
            });

            services.Configure<AspNetCoreInstrumentationOptions>(options => options.RecordException = true);
            services.Configure<OpenTelemetryLoggerOptions>(opt =>
            {
                opt.IncludeScopes = true;
                opt.ParseStateValues = true;
                opt.IncludeFormattedMessage = true;
            });
            ActivitySource = new ActivitySource(appSettings?.DistributedTracing?.Jaeger?.ServiceName ?? string.Empty);

            return services;
        }


    }
}
