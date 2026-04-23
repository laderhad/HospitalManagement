using HospitalManagement.Domain.Common;

namespace HospitalManagement.Domain.Entities;

public class Patient : BaseAuditableEntity
{
    public required string FirstName { get; set; }
    public required string LastName { get; set; }
    public DateTime DateOfBirth { get; set; }
    public required string Gender { get; set; }
    public required string ContactNumber { get; set; }
    public required string Address { get; set; }
    public Guid UserId { get; set; }
    public User? User { get; set; }
}
