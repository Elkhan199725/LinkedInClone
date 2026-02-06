using MediatR;

namespace Application.Posts.Commands;

public sealed record DeleteCommentCommand(
    Guid PostId,
    Guid CommentId,
    Guid UserId
) : IRequest<bool>;
