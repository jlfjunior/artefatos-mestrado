namespace EasyCash.Abstractions.Exceptions;

public sealed class BusinessException : Exception
{
    public string Code { get; set; }

    public BusinessException(string message)
        : base(message)
    {
    }

    public BusinessException(Error error)
        : base(error.Detail)
    {
        Code = error.Title;
    }

    public static void ThrowIfNull(object? value, Error error)
    {
        if (value is null)
            throw new BusinessException(error);
    }
}