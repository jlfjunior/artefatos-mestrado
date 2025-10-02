using EasyCash.Abstractions.Exceptions;

namespace EasyCash.Domain.Users.Errors;

public static class UserErrors
{
    public static readonly Error NotFound = new(
        "User.Found",
        "The user with the specified identifier was not found");

    public static readonly Error InvalidCredentials = new(
        "User.InvalidCredentials",
        "The provided credentials were invalid");

    public static readonly Error UserAlreadyExists = new(
        "User.AlreadyExists",
        "The user with the specified entry already exists");

    public static readonly Error NoRolesGivenToUser = new(
        "User.NoRoles",
        "No roles given to user");
}