using Application.Common.Exceptions;
using Application.Common.Interfaces;
using Application.Network.Commands;
using MediatR;

namespace Application.Network.Handlers;

public sealed class RemoveConnectionCommandHandler : IRequestHandler<RemoveConnectionCommand, bool>
{
    private readonly IConnectionRepository _connectionRepository;
    private readonly IUserFollowRepository _followRepository;

    public RemoveConnectionCommandHandler(
        IConnectionRepository connectionRepository,
        IUserFollowRepository followRepository)
    {
        _connectionRepository = connectionRepository;
        _followRepository = followRepository;
    }

    public async Task<bool> Handle(RemoveConnectionCommand command, CancellationToken cancellationToken)
    {
        var areConnected = await _connectionRepository.AreConnectedAsync(
            command.CurrentUserId, command.TargetUserId, cancellationToken);

        if (!areConnected)
            throw new NotFoundException("Connection", command.TargetUserId);

        await _connectionRepository.DeleteConnectionAsync(
            command.CurrentUserId, command.TargetUserId, cancellationToken);

        await _followRepository.DeleteMutualFollowsAsync(
            command.CurrentUserId, command.TargetUserId, cancellationToken);

        return true;
    }
}
