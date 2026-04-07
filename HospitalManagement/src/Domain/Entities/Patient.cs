using HospitalManagement.Domain.Common;

namespace HospitalManagement.Domain.Entities;

public class Patient : BaseAuditableEntity
{
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public DateTime DateOfBirth { get; set; }
    public string Gender { get; set; }
    public string ContactNumber { get; set; }
    public string Address { get; set; }
    public Guid UserId { get; set; }
    public User User { get; set; }
}
