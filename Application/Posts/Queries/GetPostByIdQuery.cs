using Application.Posts.Dtos;
using MediatR;

namespace Application.Posts.Queries;

public sealed record GetPostByIdQuery(
    Guid PostId,
    Guid? CurrentUserId = null
) : IRequest<PostResponse>;
