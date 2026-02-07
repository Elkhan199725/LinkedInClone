using Application.Common.Interfaces;
using Application.Network.Dtos;
using Application.Network.Queries;
using MediatR;

namespace Application.Network.Handlers;

public sealed class GetIncomingRequestsQueryHandler : IRequestHandler<GetIncomingRequestsQuery, ConnectionRequestListResponse>
{
    private readonly IConnectionRequestRepository _requestRepository;
    private readonly IUserProfileRepository _profileRepository;

    public GetIncomingRequestsQueryHandler(
        IConnectionRequestRepository requestRepository,
        IUserProfileRepository profileRepository)
    {
        _requestRepository = requestRepository;
        _profileRepository = profileRepository;
    }

    public async Task<ConnectionRequestListResponse> Handle(GetIncomingRequestsQuery query, CancellationToken cancellationToken)
    {
        var requests = await _requestRepository.GetIncomingRequestsAsync(
            query.UserId, query.Page, query.PageSize, cancellationToken);

        var senderIds = requests.Select(r => r.SenderId).ToList();
        var profiles = await _profileRepository.GetAllAsync(p => senderIds.Contains(p.AppUserId), cancellationToken);
        var profileDict = profiles.ToDictionary(p => p.AppUserId);

        var dtos = requests.Select(r =>
        {
            profileDict.TryGetValue(r.SenderId, out var profile);
            return new ConnectionRequestDto(
                r.Id,
                new UserSummaryDto(
                    r.SenderId,
                    profile != null ? $"{profile.FirstName} {profile.LastName}" : "Unknown User",
                    profile?.Headline,
                    profile?.ProfilePhotoUrl
                ),
                r.CreatedAt
            );
        }).ToList();

        return new ConnectionRequestListResponse(dtos, query.Page, query.PageSize);
    }
}
