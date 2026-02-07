using Application.Common.Interfaces;
using Application.Network.Dtos;
using Application.Network.Queries;
using MediatR;

namespace Application.Network.Handlers;

public sealed class GetOutgoingRequestsQueryHandler : IRequestHandler<GetOutgoingRequestsQuery, ConnectionRequestListResponse>
{
    private readonly IConnectionRequestRepository _requestRepository;
    private readonly IUserProfileRepository _profileRepository;

    public GetOutgoingRequestsQueryHandler(
        IConnectionRequestRepository requestRepository,
        IUserProfileRepository profileRepository)
    {
        _requestRepository = requestRepository;
        _profileRepository = profileRepository;
    }

    public async Task<ConnectionRequestListResponse> Handle(GetOutgoingRequestsQuery query, CancellationToken cancellationToken)
    {
        var requests = await _requestRepository.GetOutgoingRequestsAsync(
            query.UserId, query.Page, query.PageSize, cancellationToken);

        var receiverIds = requests.Select(r => r.ReceiverId).ToList();
        var profiles = await _profileRepository.GetAllAsync(p => receiverIds.Contains(p.AppUserId), cancellationToken);
        var profileDict = profiles.ToDictionary(p => p.AppUserId);

        var dtos = requests.Select(r =>
        {
            profileDict.TryGetValue(r.ReceiverId, out var profile);
            return new ConnectionRequestDto(
                r.Id,
                new UserSummaryDto(
                    r.ReceiverId,
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
