using MediatR;

namespace Application.Admin.Commands;

public sealed record AdminDeleteUserCommand(
    Guid TargetUserId,
    Guid RequestingUserId
) : IRequest<bool>;
