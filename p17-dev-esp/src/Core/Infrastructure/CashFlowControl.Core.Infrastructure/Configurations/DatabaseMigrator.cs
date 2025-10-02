using CashFlowControl.Core.Domain.Entities;
using CashFlowControl.Core.Infrastructure.Contexts;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Serilog;

namespace CashFlowControl.Core.Infrastructure.Configurations
{
    public static class DatabaseMigrator
    {
        private static readonly object MigrationLock = new object();

        public static async Task ApplyMigrationsAsync(WebApplication app)
        {
            using (var scope = app.Services.CreateScope())
            {
                var configuration = scope.ServiceProvider.GetRequiredService<IConfiguration>();
                var masterConnectionString = configuration.GetConnectionString("MasterConnection") ?? "";

                var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                try
                {
                    InitializeDb(masterConnectionString);

                    lock (MigrationLock)
                    {
                        while (HasPendingMigrations(dbContext))
                        {
                            dbContext.Database.Migrate();
                        }
                    }

                    var services = scope.ServiceProvider;
                    var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
                    var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
                    var context = services.GetRequiredService<ApplicationDbContext>();

                    var email = configuration["UserAdmin:Email"] ?? string.Empty;
                    var userName = configuration["UserAdmin:UserName"] ?? string.Empty;
                    var fullName = configuration["UserAdmin:FullName"] ?? string.Empty;
                    var password = configuration["UserAdmin:Password"] ?? string.Empty;

                    var user = await userManager.FindByEmailAsync(email);
                    if (user == null)
                    {
                        user = new ApplicationUser
                        {
                            UserName = userName,
                            FullName = fullName,
                            Email = email
                        };

                        var result = await userManager.CreateAsync(user, password);
                        if (!result.Succeeded)
                            throw new Exception("error occurred creating adm user");
                    }
                }
                catch (Microsoft.Data.SqlClient.SqlException ex)
                {
                    if (ex.Number != 2714)
                    {
                        throw;
                    }
                }
                catch (Exception ex)
                {
                    Log.Error($"Error trying to migrate database: {ex.Message}");
                    throw;
                }
            }
        }

        private static bool HasPendingMigrations(ApplicationDbContext dbContext)
        {
            return dbContext.Database.GetPendingMigrations().Any();
        }

        private static void InitializeDb(string masterConnectionString)
        {
            var retryCount = 12;
            while (retryCount > 0)
            {
                try
                {
                    TestConnection(masterConnectionString);

                    return;
                }
                catch
                {
                    retryCount--;
                    Thread.Sleep(5000);
                }
            }
            throw new Exception("Failed to connect to database.");
        }

        private static void TestConnection(string connectionString)
        {
            var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();
            optionsBuilder.UseSqlServer(connectionString);

            using (var tempDbContext = new ApplicationDbContext(optionsBuilder.Options))
            {
                tempDbContext.Database.OpenConnection();
                tempDbContext.Database.CloseConnection();
            }
        }
    }
}
