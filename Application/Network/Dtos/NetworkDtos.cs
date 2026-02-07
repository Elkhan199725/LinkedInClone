namespace Application.Network.Dtos;

public sealed record UserSummaryDto(
    Guid UserId,
    string DisplayName,
    string? Headline,
    string? AvatarUrl
);

public sealed record FollowResponse(
    Guid FollowedUserId,
    bool IsFollowing,
    DateTime? FollowedAt
);

public sealed record FollowListResponse(
    IReadOnlyList<UserSummaryDto> Users,
    int Page,
    int PageSize,
    int TotalCount
);

public sealed record ConnectionRequestDto(
    Guid RequestId,
    UserSummaryDto User,
    DateTime CreatedAt
);

public sealed record ConnectionRequestListResponse(
    IReadOnlyList<ConnectionRequestDto> Requests,
    int Page,
    int PageSize
);

public sealed record ConnectionDto(
    Guid UserId,
    string DisplayName,
    string? Headline,
    string? AvatarUrl,
    DateTime ConnectedAt
);

public sealed record ConnectionListResponse(
    IReadOnlyList<ConnectionDto> Connections,
    int Page,
    int PageSize,
    int TotalCount
);
