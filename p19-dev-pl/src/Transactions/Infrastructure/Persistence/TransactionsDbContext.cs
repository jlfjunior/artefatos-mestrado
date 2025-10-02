using Application;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence;

public class TransactionsDbContext(DbContextOptions<TransactionsDbContext> options) : DbContext(options), IApplicationDbContext
{
    public DbSet<TransactionEntity> Transactions { get; set; } = default!;
}

