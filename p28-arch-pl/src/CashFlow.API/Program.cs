using System.Text.Json.Serialization;
using CashFlow.API.Configurations;
using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers().AddJsonOptions(x =>
{
    x.JsonSerializerOptions.Converters.Add(new DateOnlyJsonConverter());
    x.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
});

builder.Services
    .AddEndpointsApiExplorer()
    .AddDatabase(builder.Configuration)
    .AddSwaggerGen(c =>
    {
        c.MapType<DateOnly>(() => new OpenApiSchema
        {
            Type = "string",
            Format = "date",
            Description = "2022-03-28",
            Example = new OpenApiString("2022-03-28")
        });
    })
    .IoC();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection()
    .UseAuthorization()
    .UseApm(builder.Configuration);

app.MapControllers();

app.Run();