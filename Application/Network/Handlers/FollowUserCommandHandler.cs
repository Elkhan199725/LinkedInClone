using Application.Common.Exceptions;
using Application.Common.Interfaces;
using Application.Network.Commands;
using Application.Network.Dtos;
using Domain.Entities;
using MediatR;

namespace Application.Network.Handlers;

public sealed class FollowUserCommandHandler : IRequestHandler<FollowUserCommand, FollowResponse>
{
    private readonly IUserFollowRepository _followRepository;
    private readonly IUserProfileRepository _profileRepository;

    public FollowUserCommandHandler(
        IUserFollowRepository followRepository,
        IUserProfileRepository profileRepository)
    {
        _followRepository = followRepository;
        _profileRepository = profileRepository;
    }

    public async Task<FollowResponse> Handle(FollowUserCommand command, CancellationToken cancellationToken)
    {
        if (command.CurrentUserId == command.TargetUserId)
            throw new ForbiddenException("You cannot follow yourself.");

        var targetProfile = await _profileRepository.GetByIdAsync(command.TargetUserId, cancellationToken);
        if (targetProfile is null)
            throw new NotFoundException("User", command.TargetUserId);

        var existingFollow = await _followRepository.GetFollowAsync(
            command.CurrentUserId, command.TargetUserId, cancellationToken);

        if (existingFollow is not null)
        {
            return new FollowResponse(command.TargetUserId, true, existingFollow.CreatedAt);
        }

        var follow = new UserFollow
        {
            FollowerId = command.CurrentUserId,
            FollowedUserId = command.TargetUserId,
            CreatedAt = DateTime.UtcNow
        };

        await _followRepository.AddAsync(follow, cancellationToken);
        await _followRepository.SaveChangesAsync(cancellationToken);

        return new FollowResponse(command.TargetUserId, true, follow.CreatedAt);
    }
}
