using MediatR;

namespace Application.Network.Commands;

public sealed record CancelConnectionRequestCommand(Guid CurrentUserId, Guid RequestId) : IRequest<bool>;
