using HospitalManagement.Domain.Common;

namespace HospitalManagement.Domain.Entities;

public class Patient : BaseAuditableEntity
{
    private Patient() { }

    public Patient(string firstName, string lastName, DateTime dateOfBirth, Gender gender, ContactNumber contactNumber, PostalAddress address, string applicationUserId)
    {
        SetPersonalDetails(firstName, lastName, dateOfBirth, gender);
        SetContactDetails(contactNumber, address);
        SetApplicationUserId(applicationUserId);
    }

    public string FirstName { get; private set; } = string.Empty;
    public string LastName { get; private set; } = string.Empty;
    public DateTime DateOfBirth { get; private set; }
    public Gender Gender { get; private set; } = Gender.Unknown;
    public ContactNumber ContactNumber { get; private set; } = null!;
    public PostalAddress Address { get; private set; } = null!;
    public string ApplicationUserId { get; private set; } = string.Empty;

    public void UpdatePersonalDetails(string firstName, string lastName, DateTime dateOfBirth, Gender gender)
    {
        SetPersonalDetails(firstName, lastName, dateOfBirth, gender);
    }

    public void UpdateContactDetails(ContactNumber contactNumber, PostalAddress address)
    {
        SetContactDetails(contactNumber, address);
    }

    private void SetPersonalDetails(string firstName, string lastName, DateTime dateOfBirth, Gender gender)
    {
        if (string.IsNullOrWhiteSpace(firstName))
        {
            throw new DomainException("First name is required.", nameof(firstName));
        }

        if (string.IsNullOrWhiteSpace(lastName))
        {
            throw new DomainException("Last name is required.", nameof(lastName));
        }

        if (dateOfBirth.Date > DateTime.UtcNow.Date)
        {
            throw new DomainException("Date of birth cannot be in the future.", nameof(dateOfBirth));
        }

        FirstName = firstName;
        LastName = lastName;
        DateOfBirth = dateOfBirth;
        Gender = gender;
    }

    private void SetContactDetails(ContactNumber contactNumber, PostalAddress address)
    {
        if (contactNumber is null)
        {
            throw new DomainException("Contact number is required.", nameof(contactNumber));
        }

        if (address is null)
        {
            throw new DomainException("Address is required.", nameof(address));
        }

        ContactNumber = contactNumber;
        Address = address;
    }

    private void SetApplicationUserId(string applicationUserId)
    {
        if (string.IsNullOrWhiteSpace(applicationUserId))
        {
            throw new DomainException("Application user id is required.", nameof(applicationUserId));
        }

        ApplicationUserId = applicationUserId;
    }
}
