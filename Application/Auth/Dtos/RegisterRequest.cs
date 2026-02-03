namespace Application.Auth.Dtos;

public sealed record RegisterRequest(
    string Email,
    string Password,
    string FirstName,
    string LastName
);
