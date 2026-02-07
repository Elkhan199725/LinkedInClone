using Application.Common.Interfaces;
using Application.Network.Commands;
using Application.Network.Dtos;
using MediatR;

namespace Application.Network.Handlers;

public sealed class UnfollowUserCommandHandler : IRequestHandler<UnfollowUserCommand, FollowResponse>
{
    private readonly IUserFollowRepository _followRepository;

    public UnfollowUserCommandHandler(IUserFollowRepository followRepository)
    {
        _followRepository = followRepository;
    }

    public async Task<FollowResponse> Handle(UnfollowUserCommand command, CancellationToken cancellationToken)
    {
        var existingFollow = await _followRepository.GetFollowAsync(
            command.CurrentUserId, command.TargetUserId, cancellationToken);

        if (existingFollow is null)
        {
            return new FollowResponse(command.TargetUserId, false, null);
        }

        _followRepository.Remove(existingFollow);
        await _followRepository.SaveChangesAsync(cancellationToken);

        return new FollowResponse(command.TargetUserId, false, null);
    }
}
