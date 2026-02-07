using MediatR;

namespace Application.Network.Commands;

public sealed record AcceptConnectionRequestCommand(Guid CurrentUserId, Guid RequestId) : IRequest<bool>;
