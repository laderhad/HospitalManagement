namespace HospitalManagement.Domain.ValueObjects;

public sealed class PostalAddress : ValueObject
{
    private PostalAddress() { }

    private PostalAddress(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new DomainException("Address is required.", nameof(value));
        }

        Value = value.Trim();
    }

    public string Value { get; private set; } = string.Empty;

    public static PostalAddress From(string value)
    {
        return new PostalAddress(value);
    }

    public static implicit operator string(PostalAddress address)
    {
        return address.Value;
    }

    public override string ToString()
    {
        return Value;
    }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Value;
    }
}
