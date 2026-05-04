using CashFlow.BuildingBlocks.Behaviors;
using CashFlow.Launches.Api.Endpoints;
using CashFlow.Launches.Application.CommandHandlers;
using CashFlow.Launches.Application.Validators;
using CashFlow.Launches.Infrastructure;
using CashFlow.Launches.Infrastructure.Persistence;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddMediatR(cfg =>
{
    cfg.RegisterServicesFromAssembly(typeof(RegisterLaunchCommandHandler).Assembly);
});

builder.Services.AddValidatorsFromAssembly(typeof(RegisterLaunchCommandValidator).Assembly);

builder.Services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));

builder.Services.AddInfrastructure(builder.Configuration);

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<LaunchesDbContext>();
    dbContext.Database.Migrate();
}

app.MapLaunchEndpoints();

app.Run();

public partial class Program;
