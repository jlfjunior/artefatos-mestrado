using EasyCash.Abstractions.Interfaces;

namespace EasyCash.Domain;

public sealed class DateTimeProvider : IDateTimeProvider
{
    public DateTime UtcNow => DateTime.UtcNow;
}
