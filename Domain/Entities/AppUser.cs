using Microsoft.AspNetCore.Identity;

namespace Domain.Entities;

/// <summary>
/// Identity-only user entity. Profile data is stored in UserProfile.
/// </summary>
public class AppUser : IdentityUser<Guid>
{
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
}
