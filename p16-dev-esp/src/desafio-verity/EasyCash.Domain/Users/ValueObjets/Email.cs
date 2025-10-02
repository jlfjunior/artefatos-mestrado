using EasyCash.Abstractions;

namespace EasyCash.Domain.Users.ValueObjets;

public sealed class Email : ValueObject
{
    public string Mail { get { return _email.ToLower(); } }

    public static Email Instance(string email) => new(email);

    public Email(string email)
    {
        if (string.IsNullOrEmpty(email?.Trim()))
            return;

        _email = email;
    }

    private readonly string _email = string.Empty;

    protected override IEnumerable<object> GetAtomicValues()
    {
        yield return Mail;
    }
}
