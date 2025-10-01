using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace CashFlow.IdentifyServer.Api.Database;

public class IdentityServerDbContext : IdentityDbContext<IdentityUser<Guid>, IdentityRole<Guid>, Guid>
{
    public IdentityServerDbContext(DbContextOptions<IdentityServerDbContext> options)
        : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.HasDefaultSchema("identity");

        modelBuilder.Entity<IdentityUser<Guid>>(b =>
        {
            b.ToTable(name: "User");
        });
        modelBuilder.Entity<IdentityRole<Guid>>(b =>
        {
            b.ToTable(name: "Role");
        });
        modelBuilder.Entity<IdentityUserRole<Guid>>(b =>
        {
            b.ToTable(name: "UserRole");
        });
        modelBuilder.Entity<IdentityUserClaim<Guid>>(b =>
        {
            b.ToTable(name: "UserClaim");
        });
        modelBuilder.Entity<IdentityUserLogin<Guid>>(b =>
        {
            b.ToTable(name: "UserLogin");
        });
        modelBuilder.Entity<IdentityRoleClaim<Guid>>(b =>
        {
            b.ToTable(name: "RoleClaim");
        });
        modelBuilder.Entity<IdentityUserToken<Guid>>(b =>
        {
            b.ToTable(name: "UserToken");
        });
    }
}
