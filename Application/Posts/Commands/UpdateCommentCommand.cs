using Application.Posts.Dtos;
using MediatR;

namespace Application.Posts.Commands;

public sealed record UpdateCommentCommand(
    Guid PostId,
    Guid CommentId,
    Guid UserId,
    UpdateCommentRequest Request
) : IRequest<CommentResponse>;
