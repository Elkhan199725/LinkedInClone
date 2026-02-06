using MediatR;

namespace Application.Profiles.Commands;

public sealed record DeleteMyAccountCommand(
    Guid UserId
) : IRequest<bool>;
