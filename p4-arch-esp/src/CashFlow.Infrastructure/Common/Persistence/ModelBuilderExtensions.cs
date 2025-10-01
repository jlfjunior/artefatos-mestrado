using Microsoft.EntityFrameworkCore;

namespace CashFlow.Infrastructure.Common.Persistence;

public static class ModelBuilderExtensions
{
    public static ModelBuilder UseStringEnums(this ModelBuilder modelBuilder)
    {
        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            var properties = entityType.ClrType.GetProperties()
                .Where(p => p.PropertyType.IsEnum);

            foreach (var property in properties)
            {
                modelBuilder.Entity(entityType.Name).Property(property.Name)
                    .HasConversion<string>();
            }
        }
        return modelBuilder;
    }
}
