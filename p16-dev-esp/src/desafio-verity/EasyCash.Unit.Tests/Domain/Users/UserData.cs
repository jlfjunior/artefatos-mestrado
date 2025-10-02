using EasyCash.Domain.Users.Entities;
using EasyCash.Domain.Users.ValueObjets;

namespace EasyCash.Unit.Tests.Domain.Users;

internal static class UserData
{
    public static UserEntity Create() => UserEntity.Create(FirstName, LastName, Email);

    public static readonly FirstName FirstName = new("First");
    public static readonly LastName LastName = new("Last");
    public static readonly Email Email = new("test@test.com");
}
