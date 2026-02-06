namespace Application.Auth.Dtos;

public sealed record ForgotPasswordRequest(string Email);

public sealed record ForgotPasswordResponse(string Message);

public sealed record ResetPasswordRequest(string Email, string Code, string NewPassword);
