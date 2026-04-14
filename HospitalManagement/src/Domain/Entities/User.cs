using HospitalManagement.Domain.Common;

namespace HospitalManagement.Domain.Entities;

public class User : BaseAuditableEntity
{
    private User() { }

    public User(string username, string passwordHash, UserRole role)
    {
        if (string.IsNullOrWhiteSpace(username))
        {
            throw new ArgumentException("Username is required.", nameof(username));
        }

        if (string.IsNullOrWhiteSpace(passwordHash))
        {
            throw new ArgumentException("Password hash is required.", nameof(passwordHash));
        }

        Username = username;
        PasswordHash = passwordHash;
        Role = role;
    }

    public string Username { get; private set; } = string.Empty;
    public string PasswordHash { get; private set; } = string.Empty;
    public UserRole Role { get; private set; }

    public void ChangeRole(UserRole role)
    {
        Role = role;
    }
}
