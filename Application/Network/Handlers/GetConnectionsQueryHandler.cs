using Application.Common.Interfaces;
using Application.Network.Dtos;
using Application.Network.Queries;
using MediatR;

namespace Application.Network.Handlers;

public sealed class GetConnectionsQueryHandler : IRequestHandler<GetConnectionsQuery, ConnectionListResponse>
{
    private readonly IConnectionRepository _connectionRepository;
    private readonly IUserProfileRepository _profileRepository;

    public GetConnectionsQueryHandler(
        IConnectionRepository connectionRepository,
        IUserProfileRepository profileRepository)
    {
        _connectionRepository = connectionRepository;
        _profileRepository = profileRepository;
    }

    public async Task<ConnectionListResponse> Handle(GetConnectionsQuery query, CancellationToken cancellationToken)
    {
        var connections = await _connectionRepository.GetConnectionsAsync(
            query.UserId, query.Page, query.PageSize, cancellationToken);
        var totalCount = await _connectionRepository.GetConnectionsCountAsync(query.UserId, cancellationToken);

        var connectedUserIds = connections.Select(c => c.ConnectedUserId).ToList();
        var profiles = await _profileRepository.GetAllAsync(p => connectedUserIds.Contains(p.AppUserId), cancellationToken);
        var profileDict = profiles.ToDictionary(p => p.AppUserId);

        var dtos = connections.Select(c =>
        {
            profileDict.TryGetValue(c.ConnectedUserId, out var profile);
            return new ConnectionDto(
                c.ConnectedUserId,
                profile != null ? $"{profile.FirstName} {profile.LastName}" : "Unknown User",
                profile?.Headline,
                profile?.ProfilePhotoUrl,
                c.CreatedAt
            );
        }).ToList();

        return new ConnectionListResponse(dtos, query.Page, query.PageSize, totalCount);
    }
}
