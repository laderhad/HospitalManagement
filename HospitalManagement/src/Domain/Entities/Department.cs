using HospitalManagement.Domain.Common;

namespace HospitalManagement.Domain.Entities;

public class Department : BaseAuditableEntity
{
    public string Name { get; set; }
    public ICollection<Doctor> Doctors { get; set; } = new List<Doctor>();
}
