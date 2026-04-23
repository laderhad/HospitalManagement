using HospitalManagement.Domain.Common;

namespace HospitalManagement.Domain.Entities;

public class Doctor : BaseAuditableEntity
{
    private Doctor() { }

    public Doctor(string firstName, string lastName, Guid departmentId, ContactNumber contactNumber, string applicationUserId, string? specialty = null, string? title = null)
    {
        SetName(firstName, lastName);
        ChangeDepartment(departmentId);
        UpdateContactNumber(contactNumber);
        SetApplicationUserId(applicationUserId);
        Specialty = specialty;
        Title = title;
        IsActive = true;
    }

    public string FirstName { get; private set; } = string.Empty;
    public string LastName { get; private set; } = string.Empty;
    public Guid DepartmentId { get; private set; }
    public Department? Department { get; set; }
    public ContactNumber ContactNumber { get; private set; } = null!;
    public string ApplicationUserId { get; private set; } = string.Empty;
    public string? Specialty { get; private set; }
    public string? Title { get; private set; }
    public bool IsActive { get; private set; }

    public void Rename(string firstName, string lastName)
    {
        SetName(firstName, lastName);
    }

    public void ChangeDepartment(Guid departmentId)
    {
        if (departmentId == Guid.Empty)
        {
            throw new DomainException("Department id is required.", nameof(departmentId));
        }

        DepartmentId = departmentId;
    }

    public void UpdateContactNumber(ContactNumber contactNumber)
    {
        if (contactNumber is null)
        {
            throw new DomainException("Contact number is required.", nameof(contactNumber));
        }

        ContactNumber = contactNumber;
    }

    public void UpdateProfessionalDetails(string? specialty, string? title)
    {
        Specialty = specialty;
        Title = title;
    }

    public void Activate()
    {
        IsActive = true;
    }

    public void Deactivate()
    {
        IsActive = false;
    }

    private void SetName(string firstName, string lastName)
    {
        if (string.IsNullOrWhiteSpace(firstName))
        {
            throw new DomainException("First name is required.", nameof(firstName));
        }

        if (string.IsNullOrWhiteSpace(lastName))
        {
            throw new DomainException("Last name is required.", nameof(lastName));
        }

        FirstName = firstName;
        LastName = lastName;
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
