using Application.Posts.Dtos;
using MediatR;

namespace Application.Posts.Commands;

public sealed record AddCommentCommand(
    Guid PostId,
    Guid AuthorId,
    AddCommentRequest Request
) : IRequest<CommentResponse>;
