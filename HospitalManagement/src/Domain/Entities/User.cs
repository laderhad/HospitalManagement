using HospitalManagement.Domain.Common;

namespace HospitalManagement.Domain.Entities;

public class User : BaseAuditableEntity
{
    public string Username { get; set; }
    public string PasswordHash { get; set; }
    public string Role { get; set; } // e.g., "Admin", "Doctor", "Patient"
}
