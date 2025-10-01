namespace CashFlow.Infrastructure.Account
{
    public static class AccountCacheKeys
    {
        public static string AccountsByUserKey(Guid userId)
        {
            string accountsCacheKeyFormat = "accounts-userId:{0}";
            return string.Format(accountsCacheKeyFormat, userId);
        }

        public static string AccountBalanceKey(Guid accountId)
        {
            string accountBalanceCacheKeyFormat = "balance-accountId:{0}";
            return string.Format(accountBalanceCacheKeyFormat, accountId);
        }

        public static string DailyBalanceByAccount(DateOnly date, Guid accountId)
        {
            const string accountDailyBalanceCacheKeyFormat = "daily-balance:{0}:accountId:{1}";
            return string.Format(accountDailyBalanceCacheKeyFormat, date.ToString("yyyy-MM-dd"), accountId);
        }
    }
}
