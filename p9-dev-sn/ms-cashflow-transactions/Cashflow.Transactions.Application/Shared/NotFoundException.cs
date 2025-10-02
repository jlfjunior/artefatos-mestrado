namespace Cashflow.Transactions.Application.Shared
{
    public class NotFoundException : Exception
    {
        public NotFoundException(string message) : base(message) { }
    }
}
