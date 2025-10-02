namespace EasyCash.Abstractions.Exceptions;

public record Error(string Title, string Detail)
{
    public static readonly Error None = new(string.Empty, string.Empty);

    public static readonly Error NullValue = new("Error.NullValue", "Null value was provided");
}