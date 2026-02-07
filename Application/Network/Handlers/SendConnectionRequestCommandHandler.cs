using Application.Common.Exceptions;
using Application.Common.Interfaces;
using Application.Network.Commands;
using Application.Network.Dtos;
using Domain.Entities;
using Domain.Enums;
using MediatR;

namespace Application.Network.Handlers;

public sealed class SendConnectionRequestCommandHandler : IRequestHandler<SendConnectionRequestCommand, ConnectionRequestDto>
{
    private readonly IConnectionRequestRepository _requestRepository;
    private readonly IConnectionRepository _connectionRepository;
    private readonly IUserProfileRepository _profileRepository;

    public SendConnectionRequestCommandHandler(
        IConnectionRequestRepository requestRepository,
        IConnectionRepository connectionRepository,
        IUserProfileRepository profileRepository)
    {
        _requestRepository = requestRepository;
        _connectionRepository = connectionRepository;
        _profileRepository = profileRepository;
    }

    public async Task<ConnectionRequestDto> Handle(SendConnectionRequestCommand command, CancellationToken cancellationToken)
    {
        if (command.SenderId == command.ReceiverId)
            throw new ForbiddenException("You cannot send a connection request to yourself.");

        var receiverProfile = await _profileRepository.GetByIdAsync(command.ReceiverId, cancellationToken);
        if (receiverProfile is null)
            throw new NotFoundException("User", command.ReceiverId);

        var alreadyConnected = await _connectionRepository.AreConnectedAsync(
            command.SenderId, command.ReceiverId, cancellationToken);
        if (alreadyConnected)
            throw new ForbiddenException("You are already connected with this user.");

        var existingRequest = await _requestRepository.GetPendingBetweenUsersAsync(
            command.SenderId, command.ReceiverId, cancellationToken);
        if (existingRequest is not null)
            throw new ForbiddenException("A pending connection request already exists between you and this user.");

        var request = new ConnectionRequest
        {
            SenderId = command.SenderId,
            ReceiverId = command.ReceiverId,
            Status = ConnectionRequestStatus.Pending,
            CreatedAt = DateTime.UtcNow
        };

        await _requestRepository.AddAsync(request, cancellationToken);
        await _requestRepository.SaveChangesAsync(cancellationToken);

        var senderProfile = await _profileRepository.GetByIdAsync(command.SenderId, cancellationToken);

        return new ConnectionRequestDto(
            request.Id,
            new UserSummaryDto(
                command.ReceiverId,
                $"{receiverProfile.FirstName} {receiverProfile.LastName}",
                receiverProfile.Headline,
                receiverProfile.ProfilePhotoUrl
            ),
            request.CreatedAt
        );
    }
}
