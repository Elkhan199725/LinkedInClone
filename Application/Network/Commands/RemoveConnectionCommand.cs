using MediatR;

namespace Application.Network.Commands;

public sealed record RemoveConnectionCommand(Guid CurrentUserId, Guid TargetUserId) : IRequest<bool>;
