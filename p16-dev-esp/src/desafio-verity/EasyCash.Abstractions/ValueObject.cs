namespace EasyCash.Abstractions;

public abstract class ValueObject
{
    protected abstract IEnumerable<object> GetAtomicValues();

    public override bool Equals(object obj)
    {
        if (obj == null || GetType() != obj.GetType())
            return false;
        ValueObject valueObject = (ValueObject)obj;
        return GetAtomicValues().SequenceEqual(valueObject.GetAtomicValues());
    }

    public override int GetHashCode() => GetAtomicValues().Aggregate(1, (current, obj) => current * 23 + (obj != null ? obj.GetHashCode() : 0));

    public static bool operator ==(ValueObject a, ValueObject b)
    {
        if (a is null && b is null)
            return true;

        return a is not null && (object)b != null && a.Equals(b);
    }

    public static bool operator !=(ValueObject a, ValueObject b) => !(a == b);
}
