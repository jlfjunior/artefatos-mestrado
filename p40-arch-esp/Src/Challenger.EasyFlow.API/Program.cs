using Challenger.EasyFlow.API.Features;
using Challenger.EasyFlow.API.Middlewares;
using Challenger.EasyFlow.Application;
using Challenger.EasyFlow.Infrastructure.Extensions;
using FluentValidation;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddProblemDetails()
    .AddExceptionHandler<GlobalExceptionHandler>()
    .AddOpenApi()
    .AddPersistence(builder.Configuration, builder.Environment)
    .AddEventBus(builder.Configuration)
    .AddValidatorsFromAssembly(typeof(IApplicationMarker).Assembly);

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

app.UseHttpsRedirection();
app.UseExceptionHandler();
app.MapEndpoints();

app.Run();