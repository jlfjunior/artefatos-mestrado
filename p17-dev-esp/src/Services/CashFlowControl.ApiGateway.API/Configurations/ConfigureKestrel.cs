using System.Security.Authentication;

namespace CashFlowControl.ApiGateway.API.Configurations
{
    public static class ConfigureKestrel
    {
        public static void Configure(WebApplicationBuilder builder)
        {
            var certPath = builder.Configuration["Kestrel:Certificates:Default:Path"] ?? "";
            var certPassword = builder.Configuration["Kestrel:Certificates:Default:Password"] ?? "";

            if (string.IsNullOrEmpty(certPath) || string.IsNullOrEmpty(certPassword))
            {
                throw new InvalidOperationException("Certificate or password not configured.");
            }

            builder.WebHost.ConfigureKestrel(options =>
            {
                options.ConfigureHttpsDefaults(httpsOptions =>
                {
                    httpsOptions.SslProtocols = SslProtocols.Tls12 | SslProtocols.Tls13;
                });

                options.ListenAnyIP(5230);
                options.ListenAnyIP(7144, listenOptions =>
                {
                    listenOptions.UseHttps(certPath, certPassword);
                });
            });
        }
    }
}
