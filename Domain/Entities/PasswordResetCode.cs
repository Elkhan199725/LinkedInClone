using Domain.Common;

namespace Domain.Entities;

public class PasswordResetCode : BaseEntity
{
    public Guid UserId { get; set; }
    public AppUser User { get; set; } = null!;
    public string CodeHash { get; set; } = null!;
    public DateTime ExpiresAtUtc { get; set; }
    public int Attempts { get; set; }
    public DateTime? LastSentAtUtc { get; set; }
    public bool IsUsed { get; set; }
}
