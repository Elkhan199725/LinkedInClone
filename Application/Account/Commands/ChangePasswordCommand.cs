using Application.Account.Dtos;
using MediatR;

namespace Application.Account.Commands;

public sealed record ChangePasswordCommand(Guid UserId, ChangePasswordRequest Request) : IRequest<bool>;
