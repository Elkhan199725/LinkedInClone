namespace Application.Profiles.Dtos;

public sealed record MyProfileResponse(
    Guid AppUserId,
    string FirstName,
    string LastName,
    string? Headline,
    string? About,
    string? Location,
    string? ProfilePhotoUrl,
    string? CoverPhotoUrl,
    bool IsPublic,
    DateTime CreatedAt,
    DateTime? UpdatedAt
);
