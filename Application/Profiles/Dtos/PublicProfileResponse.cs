namespace Application.Profiles.Dtos;

public sealed record PublicProfileResponse(
    Guid AppUserId,
    string FirstName,
    string LastName,
    string? Headline,
    string? About,
    string? Location,
    string? ProfilePhotoUrl,
    string? CoverPhotoUrl
);
