namespace Application.Auth.Dtos;

public sealed record AuthResponse(
    Guid UserId,
    string Email,
    string[] Roles,
    string AccessToken
);
