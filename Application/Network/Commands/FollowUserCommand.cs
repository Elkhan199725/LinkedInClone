using Application.Network.Dtos;
using MediatR;

namespace Application.Network.Commands;

public sealed record FollowUserCommand(Guid CurrentUserId, Guid TargetUserId) : IRequest<FollowResponse>;
