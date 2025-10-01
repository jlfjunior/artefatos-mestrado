using CashFlow.Api;
using CashFlow.Api.Endpoints;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.ConfigureServices();

var app = builder.Build();

app.MapAllEndpoints();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();

    // TODO: Deve ser migrado para executar em pipeline de deploy    
    using var scope = app.Services.CreateScope();
    var services = scope.ServiceProvider;
    var context = services.GetRequiredService<CashFlow.Infrastructure.Common.Persistence.CashFlowDbContext>();
    await context.Database.MigrateAsync();
}
// https://stackoverflow.com/questions/79682092/apphost-testing-authorization-header-missing
// Comentado por conta de um bug no aspire testing
//app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();

await app.RunAsync();

