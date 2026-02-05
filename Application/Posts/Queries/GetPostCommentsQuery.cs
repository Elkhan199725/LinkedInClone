using Application.Posts.Dtos;
using MediatR;

namespace Application.Posts.Queries;

public sealed record GetPostCommentsQuery(
    Guid PostId,
    int Page = 1,
    int PageSize = 20
) : IRequest<IReadOnlyList<CommentResponse>>;
