using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Domain.Models.Authentication;
using Domain.Entities.Launch;

namespace Infrastructure.Persistence.Data;
public class ApplicationDbContext : IdentityDbContext<ApplicationUserModel>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<LaunchEntity> Launch { get; set; }
    public DbSet<ProductEntity> Product { get; set; }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        LaunchModelCreating(builder);
        ProductModelCreating(builder);
        LaunchProductModelCreating(builder);
    }

    private void ProductModelCreating(ModelBuilder builder)
    {
        builder.Entity<ProductEntity>()
        .HasKey(p => p.Id);

        builder.Entity<ProductEntity>()
            .Property(p => p.Name)
            .IsRequired()
            .HasMaxLength(100);

        builder.Entity<ProductEntity>()
            .Property(p => p.Price)
            .HasColumnType("decimal(18,2)");

        builder.Entity<ProductEntity>()
            .Property(p => p.Stock)
            .IsRequired();
    }
    private void LaunchModelCreating(ModelBuilder builder)
    {
        builder.Entity<LaunchEntity>()
            .HasKey(l => l.Id);

        builder.Entity<LaunchEntity>()
            .Property(l => l.Amount)
            .HasColumnType("decimal(18,2)");

        builder.Entity<LaunchEntity>()
            .Property(l => l.CreationDate)
            .IsRequired();

        builder.Entity<LaunchEntity>()
            .Property(l => l.LaunchType)
            .HasConversion<int>();

        builder.Entity<LaunchEntity>()
            .Property(l => l.Status)
            .HasConversion<int>();
        
        builder.Entity<LaunchEntity>()
            .Property(l => l.ModificationDate)
            .IsRequired();
    }

    private void LaunchProductModelCreating(ModelBuilder builder)
    {
        builder.Entity<LaunchProductEntity>()
            .HasKey(lp => new { lp.LaunchId, lp.ProductId });

        builder.Entity<LaunchProductEntity>()
            .Property(l => l.ProductQuantity)
            .IsRequired();

        builder.Entity<LaunchProductEntity>()
            .Property(lp => lp.ProductPrice)
            .HasPrecision(18, 2);

        builder.Entity<LaunchProductEntity>()
            .HasOne(lp => lp.Launch)
            .WithMany(l => l.LaunchProducts)
            .HasForeignKey(lp => lp.LaunchId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<LaunchProductEntity>()
            .HasOne(lp => lp.Product)
            .WithMany(p => p.LaunchProducts)
            .HasForeignKey(lp => lp.ProductId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}