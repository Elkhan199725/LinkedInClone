using Application.Account.Dtos;
using MediatR;

namespace Application.Account.Commands;

public sealed record ChangeEmailCommand(
    Guid UserId,
    ChangeEmailRequest Request
) : IRequest<ChangeEmailResponse>;
