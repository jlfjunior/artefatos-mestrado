using Challenger.EasyFlow.Domain.Aggregates.CashBoxAggregate;
using Challenger.EasyFlow.Domain.Aggregates.MerchantAggregate;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Challenger.EasyFlow.Infrastructure.Database.SQLServer;

internal sealed class EasyFlowContextFactory : IDesignTimeDbContextFactory<EasyFlowContext>
{
    public EasyFlowContext CreateDbContext(string[] args)
    {
        var context = new EasyFlowContext(
            new DbContextOptionsBuilder<EasyFlowContext>()
                .UseSqlServer()
                .UseSeeding((context, _) =>
                {
                    if (!context.Set<Merchant>().Any())
                        context.Set<Merchant>().Add(DatabaseInitializer.DefaultMerchant);

                    if (!context.Set<CashBox>().Any())
                        context.Set<CashBox>().Add(DatabaseInitializer.DefaultCashBox);

                    context.SaveChanges();
                })
                .UseAsyncSeeding(async (context, _, cancellationToken) =>
                {
                    if (!await context.Set<Merchant>().AnyAsync(cancellationToken))
                        await context.Set<Merchant>().AddAsync(DatabaseInitializer.DefaultMerchant, cancellationToken);

                    if (!await context.Set<CashBox>().AnyAsync(cancellationToken))
                        await context.Set<CashBox>().AddAsync(DatabaseInitializer.DefaultCashBox, cancellationToken);

                    await context.SaveChangesAsync(cancellationToken);
                })
                .Options);

        return context;
    }
}


internal static class DatabaseInitializer
{
    public static readonly Merchant DefaultMerchant = new("John Doe", "BRL", "America/Sao_Paulo");
    public static readonly CashBox DefaultCashBox = new(DefaultMerchant.Id, "Main Cash Box", 1000m);
}