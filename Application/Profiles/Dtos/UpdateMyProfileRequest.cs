namespace Application.Profiles.Dtos;

public sealed record UpdateMyProfileRequest(
    string FirstName,
    string LastName,
    string? Headline,
    string? About,
    string? Location,
    string? ProfilePhotoUrl,
    string? CoverPhotoUrl,
    bool IsPublic
);
