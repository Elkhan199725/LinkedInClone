using Application.Auth.Dtos;
using MediatR;

namespace Application.Auth.Commands;

public sealed record ResetPasswordCommand(ResetPasswordRequest Request) : IRequest<bool>;
