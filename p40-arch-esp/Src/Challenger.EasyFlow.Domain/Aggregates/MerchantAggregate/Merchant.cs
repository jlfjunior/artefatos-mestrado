using Challenger.EasyFlow.Domain.Common;

namespace Challenger.EasyFlow.Domain.Aggregates.MerchantAggregate;

public sealed class Merchant(string name, string currency, string timezone) : Entity<Guid>(Guid.CreateVersion7())
{
    public string Name { get; set => field = ValidateName(value); } = name;
    public string Currency { get; set => field = ValidateCurrency(value); } = currency;
    public string Timezone { get; set => field = ValidateTimezone(value); } = timezone;

    private static string ValidateName(string name)
    {
        ArgumentException.ThrowIfNullOrEmpty(name, nameof(name));

        return name;
    }

    private static string ValidateCurrency(string currency)
    {
        ArgumentException.ThrowIfNullOrEmpty(currency, nameof(currency));

        return currency;
    }

    private static string ValidateTimezone(string timezone)
    {
        ArgumentException.ThrowIfNullOrEmpty(timezone, nameof(timezone));

        return timezone;
    }
}
