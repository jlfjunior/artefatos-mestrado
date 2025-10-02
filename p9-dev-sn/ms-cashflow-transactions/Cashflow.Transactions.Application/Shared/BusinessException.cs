namespace Cashflow.Transactions.Application.Shared
{
    public class BusinessException : Exception
    {
        public BusinessException(string message) : base(message) { }
    }
}
