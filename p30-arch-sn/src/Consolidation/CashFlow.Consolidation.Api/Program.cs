using CashFlow.BuildingBlocks.Behaviors;
using CashFlow.Consolidation.Api.Endpoints;
using CashFlow.Consolidation.Application.QueryHandlers;
using CashFlow.Consolidation.Infrastructure;
using CashFlow.Consolidation.Infrastructure.Persistence;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddMediatR(cfg =>
{
    cfg.RegisterServicesFromAssembly(typeof(GetDailyConsolidationQueryHandler).Assembly);
});

builder.Services.AddValidatorsFromAssemblyContaining<GetDailyConsolidationQueryHandler>();
builder.Services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));

builder.Services.AddInfrastructure(builder.Configuration);

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<ConsolidationDbContext>();
    dbContext.Database.Migrate();
}

app.MapConsolidationEndpoints();

app.Run();

public partial class Program;
