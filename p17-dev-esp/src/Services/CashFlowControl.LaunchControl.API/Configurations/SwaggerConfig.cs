namespace CashFlowControl.LaunchControl.API.Configurations
{
    public static class SwaggerConfig
    {
        public static void Configure(WebApplication app)
        {
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI(options =>
                {
                    options.SwaggerEndpoint("/swagger/v1/swagger.json", "Launch Control API v1");
                });
            }
        }
    }
}
