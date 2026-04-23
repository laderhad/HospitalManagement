using HospitalManagement.Domain.Common;

namespace HospitalManagement.Domain.Entities;

public class Department : BaseAuditableEntity
{
    private Department() { }

    public Department(string name)
    {
        Rename(name);
        IsActive = true;
    }

    private readonly List<Doctor> _doctors = [];

    public string Name { get; private set; } = string.Empty;
    public bool IsActive { get; private set; }
    public IReadOnlyCollection<Doctor> Doctors => _doctors.AsReadOnly();

    public void Rename(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new DomainException("Department name is required.", nameof(name));
        }

        Name = name;
    }

    public void Activate()
    {
        IsActive = true;
    }

    public void Deactivate()
    {
        IsActive = false;
    }
}
