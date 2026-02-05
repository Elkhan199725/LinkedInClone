using Application.Posts.Dtos;
using MediatR;

namespace Application.Posts.Queries;

public sealed record GetMyPostsQuery(
    Guid AuthorId,
    int Page = 1,
    int PageSize = 10
) : IRequest<PostListResponse>;
