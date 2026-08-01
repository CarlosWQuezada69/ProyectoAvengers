namespace ProyectoAvengers.Domain.Entities;

public class User
{
    public Guid Id { get; private set; }
    public string FirstName { get; private set; } = string.Empty;
    public string LastName { get; private set; } = string.Empty;
    public string Email { get; private set; } = string.Empty;
    public string PasswordHash { get; private set; } = string.Empty;
    public string? Phone { get; private set; }
    public bool IsActive { get; private set; } = true;
    public bool EmailConfirmed { get; private set; }
    public int FailedLoginAttempts { get; private set; }
    public DateTime? LockedUntilUtc { get; private set; }
    public DateTime CreatedAt { get; private set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; private set; }
    public DateTime? LastLoginAt { get; private set; }
    public DateTime? DeletedAt { get; private set; }

    public ICollection<UserRole> UserRoles { get; private set; } = new List<UserRole>();
    public ICollection<RefreshToken> RefreshTokens { get; private set; } = new List<RefreshToken>();
    public ICollection<PasswordResetToken> PasswordResetTokens { get; private set; } = new List<PasswordResetToken>();
    public ICollection<EmailChangeRequest> EmailChangeRequests { get; private set; } = new List<EmailChangeRequest>();

    private User() { }

    public User(string firstName, string lastName, string email, string passwordHash, string? phone)
    {
        Id = Guid.NewGuid();
        FirstName = firstName;
        LastName = lastName;
        Email = email;
        PasswordHash = passwordHash;
        Phone = phone;
        IsActive = true;
        EmailConfirmed = false;
        CreatedAt = DateTime.UtcNow;
    }

    public void UpdateDetails(string firstName, string lastName, string? phone, bool isActive)
    {
        FirstName = firstName;
        LastName = lastName;
        Phone = phone;
        IsActive = isActive;
        UpdatedAt = DateTime.UtcNow;
    }

    public void RecordFailedLogin()
    {
        FailedLoginAttempts++;
        if (FailedLoginAttempts >= 5)
            LockedUntilUtc = DateTime.UtcNow.AddMinutes(15);
    }

    public void ResetFailedLogins()
    {
        FailedLoginAttempts = 0;
        LockedUntilUtc = null;
    }

    public void RecordLogin()
    {
        LastLoginAt = DateTime.UtcNow;
    }

    public void ChangePassword(string newPasswordHash)
    {
        PasswordHash = newPasswordHash;
    }

    public void ChangeEmail(string newEmail)
    {
        Email = newEmail;
    }

    public void ConfirmEmail()
    {
        EmailConfirmed = true;
    }

    public void SoftDelete()
    {
        DeletedAt = DateTime.UtcNow;
        IsActive = false;
    }

    public void AssignRoles(ICollection<Guid> roleIds)
    {
        UserRoles.Clear();
        foreach (var roleId in roleIds)
            UserRoles.Add(new UserRole { UserId = Id, RoleId = roleId });

        UpdatedAt = DateTime.UtcNow;
    }
}
