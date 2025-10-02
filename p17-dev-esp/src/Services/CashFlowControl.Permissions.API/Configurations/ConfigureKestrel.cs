namespace CashFlowControl.Permissions.API.Configurations
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
                options.ListenAnyIP(5290);
                options.ListenAnyIP(7043, listenOptions =>
                {
                    listenOptions.UseHttps(certPath, certPassword);
                });
            });
        }
    }
}
