namespace CashFlow.Infrastructure.Common.Cache
{
    public class RedLockRootOptions : Dictionary<string, RedLockOptions>
    {
        public static string SectionName => "RedLock";

        public RedLockOptions GetOptions(string key)
        {
            if (this.TryGetValue(key, out var options))
                return options;

            throw new KeyNotFoundException($"RedLock options with key '{key}' not found.");
        }
    }

    public class RedLockOptions
    {
        public int RetryTimeSeconds { get; set; }
        public int ExpiryTimeSeconds { get; set; }
        public int WaitTimeSeconds { get; set; }
    }
}
