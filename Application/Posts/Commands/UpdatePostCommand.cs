using Application.Posts.Dtos;
using MediatR;

namespace Application.Posts.Commands;

public sealed record UpdatePostCommand(
    Guid PostId,
    Guid UserId,
    UpdatePostRequest Request
) : IRequest<PostResponse>;
