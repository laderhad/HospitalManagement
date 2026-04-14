using HospitalManagement.Domain.Common;

namespace HospitalManagement.Domain.Entities;

public class Patient : BaseAuditableEntity
{
    private Patient() { }

    public Patient(string firstName, string lastName, DateTime dateOfBirth, Gender gender, string contactNumber, string address, Guid userId)
    {
        if (string.IsNullOrWhiteSpace(firstName))
        {
            throw new ArgumentException("First name is required.", nameof(firstName));
        }

        if (string.IsNullOrWhiteSpace(lastName))
        {
            throw new ArgumentException("Last name is required.", nameof(lastName));
        }

        if (dateOfBirth.Date > DateTime.UtcNow.Date)
        {
            throw new ArgumentException("Date of birth cannot be in the future.", nameof(dateOfBirth));
        }

        if (string.IsNullOrWhiteSpace(contactNumber))
        {
            throw new ArgumentException("Contact number is required.", nameof(contactNumber));
        }

        if (string.IsNullOrWhiteSpace(address))
        {
            throw new ArgumentException("Address is required.", nameof(address));
        }

        if (userId == Guid.Empty)
        {
            throw new ArgumentException("User id is required.", nameof(userId));
        }

        FirstName = firstName;
        LastName = lastName;
        DateOfBirth = dateOfBirth;
        Gender = gender;
        ContactNumber = contactNumber;
        Address = address;
        UserId = userId;
    }

    public string FirstName { get; private set; } = string.Empty;
    public string LastName { get; private set; } = string.Empty;
    public DateTime DateOfBirth { get; private set; }
    public Gender Gender { get; private set; } = Gender.Unknown;
    public string ContactNumber { get; private set; } = string.Empty;
    public string Address { get; private set; } = string.Empty;
    public Guid UserId { get; private set; }
    public User? User { get; set; }

    public void UpdateContactDetails(string contactNumber, string address)
    {
        if (string.IsNullOrWhiteSpace(contactNumber))
        {
            throw new ArgumentException("Contact number is required.", nameof(contactNumber));
        }

        if (string.IsNullOrWhiteSpace(address))
        {
            throw new ArgumentException("Address is required.", nameof(address));
        }

        ContactNumber = contactNumber;
        Address = address;
    }
}
