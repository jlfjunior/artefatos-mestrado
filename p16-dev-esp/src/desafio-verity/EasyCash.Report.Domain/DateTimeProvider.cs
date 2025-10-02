using EasyCash.Abstractions.Interfaces;

namespace EasyCash.Report.Domain;

public sealed class DateTimeProvider : IDateTimeProvider
{
    public DateTime UtcNow => DateTime.UtcNow;
}
