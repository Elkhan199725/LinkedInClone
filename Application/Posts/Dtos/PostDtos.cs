using Domain.Enums;

namespace Application.Posts.Dtos;

// ==================== Request DTOs ====================

public sealed record CreatePostRequest(
    string? Text,
    PostVisibility Visibility = PostVisibility.Public
);

public sealed record UpdatePostRequest(
    string? Text,
    PostVisibility Visibility
);

public sealed record AddPostMediaRequest(
    PostMediaType Type,
    string Url,
    string? PublicId = null,
    int Order = 0,
    int? Width = null,
    int? Height = null,
    int? Duration = null
);

public sealed record ReactToPostRequest(
    ReactionType Type
);

public sealed record AddCommentRequest(
    string Text,
    Guid? ParentCommentId = null
);

// ==================== Response DTOs ====================

public sealed record PostResponse(
    Guid Id,
    Guid AuthorId,
    string AuthorName,
    string? AuthorProfilePhotoUrl,
    string? Text,
    PostVisibility Visibility,
    IReadOnlyList<PostMediaResponse> Media,
    int ReactionsCount,
    int CommentsCount,
    ReactionType? CurrentUserReaction,
    DateTime CreatedAt,
    DateTime? UpdatedAt
);

public sealed record PostMediaResponse(
    Guid Id,
    PostMediaType Type,
    string Url,
    int Order,
    int? Width,
    int? Height,
    int? Duration
);

public sealed record ReactionResponse(
    Guid Id,
    Guid PostId,
    Guid UserId,
    ReactionType Type,
    DateTime CreatedAt
);

public sealed record CommentResponse(
    Guid Id,
    Guid PostId,
    Guid AuthorId,
    string AuthorName,
    string? AuthorProfilePhotoUrl,
    string Text,
    Guid? ParentCommentId,
    IReadOnlyList<CommentResponse> Replies,
    DateTime CreatedAt,
    DateTime? UpdatedAt
);

public sealed record PostListResponse(
    IReadOnlyList<PostResponse> Posts,
    int Page,
    int PageSize,
    bool HasMore
);

public sealed record ReactionSummaryResponse(
    ReactionType Type,
    int Count
);
