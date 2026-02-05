using Application.Posts.Dtos;
using MediatR;

namespace Application.Posts.Commands;

public sealed record CreatePostCommand(
    Guid AuthorId,
    CreatePostRequest Request
) : IRequest<PostResponse>;
