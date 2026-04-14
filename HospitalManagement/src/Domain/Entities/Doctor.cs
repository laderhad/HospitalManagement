using HospitalManagement.Domain.Common;

namespace HospitalManagement.Domain.Entities;

public class Doctor : BaseAuditableEntity
{
    public required string FirstName { get; set; }
    public required string LastName { get; set; }
    public Guid DepartmentId { get; set; }
    public Department? Department { get; set; }
    public required string ContactNumber { get; set; }
    public Guid UserId { get; set; }
    public User? User { get; set; }
}
