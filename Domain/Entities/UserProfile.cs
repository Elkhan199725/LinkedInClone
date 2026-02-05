namespace Domain.Entities;


public class UserProfile
{
    public Guid AppUserId { get; set; }
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