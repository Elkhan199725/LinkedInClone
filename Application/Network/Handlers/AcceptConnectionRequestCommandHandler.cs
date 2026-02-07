using Application.Common.Exceptions;
using Application.Common.Interfaces;
using Application.Network.Commands;
using Domain.Entities;
using Domain.Enums;
using MediatR;

namespace Application.Network.Handlers;

public sealed class AcceptConnectionRequestCommandHandler : IRequestHandler<AcceptConnectionRequestCommand, bool>
{
    private readonly IConnectionRequestRepository _requestRepository;
    private readonly IConnectionRepository _connectionRepository;
    private readonly IUserFollowRepository _followRepository;

    public AcceptConnectionRequestCommandHandler(
        IConnectionRequestRepository requestRepository,
        IConnectionRepository connectionRepository,
        IUserFollowRepository followRepository)
    {
        _requestRepository = requestRepository;
        _connectionRepository = connectionRepository;
        _followRepository = followRepository;
    }

    public async Task<bool> Handle(AcceptConnectionRequestCommand command, CancellationToken cancellationToken)
    {
        var request = await _requestRepository.GetByIdWithUsersAsync(command.RequestId, cancellationToken);

        if (request is null)
            throw new NotFoundException("ConnectionRequest", command.RequestId);

        if (request.ReceiverId != command.CurrentUserId)
            throw new ForbiddenException("You can only accept requests sent to you.");

        if (request.Status != ConnectionRequestStatus.Pending)
            throw new ForbiddenException("This request is no longer pending.");

        request.Status = ConnectionRequestStatus.Accepted;
        request.RespondedAt = DateTime.UtcNow;
        request.UpdatedAt = DateTime.UtcNow;

        _requestRepository.Update(request);

        var connection1 = new Connection
        {
            UserId = request.SenderId,
            ConnectedUserId = request.ReceiverId,
            CreatedAt = DateTime.UtcNow
        };

        var connection2 = new Connection
        {
            UserId = request.ReceiverId,
            ConnectedUserId = request.SenderId,
            CreatedAt = DateTime.UtcNow
        };

        await _connectionRepository.AddAsync(connection1, cancellationToken);
        await _connectionRepository.AddAsync(connection2, cancellationToken);

        var followSenderToReceiver = await _followRepository.GetFollowAsync(
            request.SenderId, request.ReceiverId, cancellationToken);
        if (followSenderToReceiver is null)
        {
            await _followRepository.AddAsync(new UserFollow
            {
                FollowerId = request.SenderId,
                FollowedUserId = request.ReceiverId,
                CreatedAt = DateTime.UtcNow
            }, cancellationToken);
        }

        var followReceiverToSender = await _followRepository.GetFollowAsync(
            request.ReceiverId, request.SenderId, cancellationToken);
        if (followReceiverToSender is null)
        {
            await _followRepository.AddAsync(new UserFollow
            {
                FollowerId = request.ReceiverId,
                FollowedUserId = request.SenderId,
                CreatedAt = DateTime.UtcNow
            }, cancellationToken);
        }

        await _connectionRepository.SaveChangesAsync(cancellationToken);

        return true;
    }
}
