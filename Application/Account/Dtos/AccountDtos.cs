namespace Application.Account.Dtos;

public sealed record ChangeEmailRequest(string NewEmail);

public sealed record ChangeEmailResponse(Guid UserId, string Email);
