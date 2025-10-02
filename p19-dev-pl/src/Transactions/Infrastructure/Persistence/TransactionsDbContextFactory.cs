using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace Infrastructure.Persistence;

public class TransactionsDbContextFactory(IConfiguration configuration) : IDesignTimeDbContextFactory<TransactionsDbContext>
{
    public readonly IConfiguration Configuration = configuration;

    public TransactionsDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<TransactionsDbContext>();
        var connectionString = Configuration!.GetConnectionString("DefaultConnection");
        optionsBuilder.UseNpgsql(connectionString);

        return new TransactionsDbContext(optionsBuilder.Options);
    }
}
