namespace EasyCash.Abstractions.Interfaces;

public interface IDateTimeProvider
{
    DateTime UtcNow { get; }
}
