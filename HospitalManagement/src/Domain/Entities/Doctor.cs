using HospitalManagement.Domain.Common;

namespace HospitalManagement.Domain.Entities;

public class Doctor : BaseAuditableEntity
{
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public Guid DepartmentId { get; set; }
    public Department Department { get; set; }
    public string ContactNumber { get; set; }
    public Guid UserId { get; set; }
    public User User { get; set; }
}
