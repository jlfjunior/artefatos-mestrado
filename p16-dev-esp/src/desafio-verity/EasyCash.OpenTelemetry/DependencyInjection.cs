using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Npgsql;
using OpenTelemetry.Logs;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

namespace EasyCash.OpenTelemetry;

public static class DependencyInjection
{
    public static IHostApplicationBuilder AddOpenTelemetryProvider(this IHostApplicationBuilder builder, string serviceName)
    {
        var resourceBuilder = ResourceBuilder.CreateDefault().AddService(serviceName);

        builder.Logging.AddOpenTelemetry(x =>
        {
            x.SetResourceBuilder(resourceBuilder);

            x.IncludeScopes = true;
            x.IncludeFormattedMessage = true;
        });

        builder.Services.AddOpenTelemetry()
        .WithMetrics(m =>
        {
            m.SetResourceBuilder(resourceBuilder);

            m.AddAspNetCoreInstrumentation()
                .AddRuntimeInstrumentation()
                .AddHttpClientInstrumentation()
                .AddEventCountersInstrumentation()
                ;

        }).WithTracing(t =>
        {

            t.SetResourceBuilder(resourceBuilder);

            if (builder.Environment.IsDevelopment())
                t.SetSampler<AlwaysOnSampler>();

            t.AddAspNetCoreInstrumentation()
                .AddHttpClientInstrumentation()
                .AddNpgsql()
                .AddRedisInstrumentation()
                .AddEntityFrameworkCoreInstrumentation()
                .AddSource("EasyCash.Command")
                .AddSource("EasyCash.Query")
                .AddSource("EasyCash.Api")
                ;
        });

        builder.AddOpenTelemetryExporters();

        return builder;
    }

    private static IHostApplicationBuilder AddOpenTelemetryExporters(this IHostApplicationBuilder builder)
    {
        var useOtlpExporter = !string.IsNullOrWhiteSpace(builder.Configuration["OTEL_EXPORTER_OTLP_ENDPOINT"]);

        if (useOtlpExporter)
        {
            builder.Services.Configure<OpenTelemetryLoggerOptions>(logging => logging.AddOtlpExporter());
            builder.Services.ConfigureOpenTelemetryMeterProvider(metrics => metrics.AddOtlpExporter());
            builder.Services.ConfigureOpenTelemetryTracerProvider(tracing => tracing.AddOtlpExporter());
        }

        return builder;
    }
}
