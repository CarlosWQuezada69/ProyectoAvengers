namespace ProyectoAvengers.Domain.Entities;

public class EmailChangeRequest
{
    public Guid Id { get; private set; }
    public Guid UserId { get; private set; }
    public string NewEmail { get; private set; } = string.Empty;
    public string Token { get; private set; } = string.Empty;
    public DateTime ExpiresAt { get; private set; }
    public DateTime? ConfirmedAt { get; private set; }
    public DateTime CreatedAt { get; private set; } = DateTime.UtcNow;

    public User User { get; private set; } = null!;

    private EmailChangeRequest() { }

    public EmailChangeRequest(Guid userId, string newEmail, string token, DateTime expiresAt)
    {
        Id = Guid.NewGuid();
        UserId = userId;
        NewEmail = newEmail;
        Token = token;
        ExpiresAt = expiresAt;
        CreatedAt = DateTime.UtcNow;
    }

    public void Confirm()
    {
        ConfirmedAt = DateTime.UtcNow;
    }
}
