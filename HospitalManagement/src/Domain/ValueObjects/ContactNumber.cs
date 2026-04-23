namespace HospitalManagement.Domain.ValueObjects;

public sealed class ContactNumber : ValueObject
{
    private ContactNumber() { }

    private ContactNumber(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new DomainException("Contact number is required.", nameof(value));
        }

        Value = value.Trim();
    }

    public string Value { get; private set; } = string.Empty;

    public static ContactNumber From(string value)
    {
        return new ContactNumber(value);
    }

    public static implicit operator string(ContactNumber contactNumber)
    {
        return contactNumber.Value;
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
