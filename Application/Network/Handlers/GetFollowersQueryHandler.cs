using Application.Common.Interfaces;
using Application.Network.Dtos;
using Application.Network.Queries;
using MediatR;

namespace Application.Network.Handlers;

public sealed class GetFollowersQueryHandler : IRequestHandler<GetFollowersQuery, FollowListResponse>
{
    private readonly IUserFollowRepository _followRepository;
    private readonly IUserProfileRepository _profileRepository;

    public GetFollowersQueryHandler(
        IUserFollowRepository followRepository,
        IUserProfileRepository profileRepository)
    {
        _followRepository = followRepository;
        _profileRepository = profileRepository;
    }

    public async Task<FollowListResponse> Handle(GetFollowersQuery query, CancellationToken cancellationToken)
    {
        var follows = await _followRepository.GetFollowersAsync(query.UserId, query.Page, query.PageSize, cancellationToken);
        var totalCount = await _followRepository.GetFollowersCountAsync(query.UserId, cancellationToken);

        var userIds = follows.Select(f => f.FollowerId).ToList();
        var profiles = await _profileRepository.GetAllAsync(p => userIds.Contains(p.AppUserId), cancellationToken);
        var profileDict = profiles.ToDictionary(p => p.AppUserId);

        var users = follows.Select(f =>
        {
            profileDict.TryGetValue(f.FollowerId, out var profile);
            return new UserSummaryDto(
                f.FollowerId,
                profile != null ? $"{profile.FirstName} {profile.LastName}" : "Unknown User",
                profile?.Headline,
                profile?.ProfilePhotoUrl
            );
        }).ToList();

        return new FollowListResponse(users, query.Page, query.PageSize, totalCount);
    }
}
