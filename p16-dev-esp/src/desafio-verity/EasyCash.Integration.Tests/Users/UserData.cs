using EasyCash.Command.Users.Register;

namespace EasyCash.Integration.Tests.Users;

internal static class UserData
{
    public static RegisterUserCommand RegisterTestUserRequest = new("test@test.com", "test", "test", "12345");
}
