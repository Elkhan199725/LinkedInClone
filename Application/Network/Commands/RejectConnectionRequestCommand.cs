using MediatR;

namespace Application.Network.Commands;

public sealed record RejectConnectionRequestCommand(Guid CurrentUserId, Guid RequestId) : IRequest<bool>;
