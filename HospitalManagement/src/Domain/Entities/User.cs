using HospitalManagement.Domain.Common;

namespace HospitalManagement.Domain.Entities;

public class User : BaseAuditableEntity
{
    public required string Username { get; set; }
    public required string PasswordHash { get; set; }
    public required string Role { get; set; } // e.g., "Admin", "Doctor", "Patient"
}
