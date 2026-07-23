namespace ProyectoAvengers.Domain.Entities;

public class EmailChangeRequest
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string NewEmail { get; set; } = string.Empty;
    public string Token { get; set; } = string.Empty;
    public DateTime ExpiresAt { get; set; }
    public DateTime? ConfirmedAt { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public User User { get; set; } = null!;
}
