using EasyCash.Command.Store.Constants;
using EasyCash.Domain.Users.Entities;
using EasyCash.Domain.Users.ValueObjets;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EasyCash.Command.Store.EntityConfigurations.Users;

internal sealed class UserConfiguration : IEntityTypeConfiguration<UserEntity>
{
    public void Configure(EntityTypeBuilder<UserEntity> builder)
    {
        builder.ToTable(TableNames.Users);

        builder.HasKey(user => user.Id);

        builder.Property(user => user.FirstName)
            .HasMaxLength(200)
            .HasConversion(firstName => firstName.Value, value => new FirstName(value))
            .HasComment("The real first name of the user");

        builder.Property(user => user.LastName)
            .HasMaxLength(200)
            .HasConversion(firstName => firstName.Value, value => new LastName(value))
            .HasComment("The real last name of the user");

        builder.Property(user => user.Email)
            .HasMaxLength(100)
            .HasConversion(email => email.Mail, value => Email.Instance(value))
            .HasComment("user e-mail");

        builder.HasIndex(user => user.Email).IsUnique();

        builder.HasIndex(user => user.IdentityId).IsUnique();
    }
}
