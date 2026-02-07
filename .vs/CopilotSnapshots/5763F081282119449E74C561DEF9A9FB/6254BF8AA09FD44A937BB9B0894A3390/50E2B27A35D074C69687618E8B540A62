namespace Domain.Entities;

/// <summary>
/// User profile with shared primary key pattern.
/// AppUserId is both the PK and FK to AppUser.
/// </summary>
public class UserProfile
{
    /// <summary>
    /// Primary key and foreign key to AppUser.Id (shared PK pattern)
    /// </summary>
    public Guid AppUserId { get; set; }

    /// <summary>
    /// Navigation property to the associated AppUser
    /// </summary>
    public AppUser AppUser { get; set; } = null!;

    public string FirstName { get; set; } = null!;
    public string LastName { get; set; } = null!;

    public string? Headline { get; set; }
    public string? About { get; set; }
    public string? Location { get; set; }

    public string? ProfilePhotoUrl { get; set; }
    public string? CoverPhotoUrl { get; set; }

    public bool IsPublic { get; set; } = true;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
}