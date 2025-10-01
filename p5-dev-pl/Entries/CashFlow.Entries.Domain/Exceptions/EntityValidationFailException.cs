namespace CashFlow.Entries.Domain.Exceptions
{
    public class EntityValidationFailException : Exception
    {
        public EntityValidationFailException(string message) : base(message)
        { }

        public EntityValidationFailException(string message, Exception innerException) : base(message, innerException)
        { }

        public EntityValidationFailException(IEnumerable<string> errors)
            : base(string.Join(" ", errors))
        { }
    }
}
