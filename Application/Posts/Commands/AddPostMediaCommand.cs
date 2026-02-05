using Application.Posts.Dtos;
using MediatR;

namespace Application.Posts.Commands;

public sealed record AddPostMediaCommand(
    Guid PostId,
    Guid UserId,
    IReadOnlyList<AddPostMediaRequest> MediaItems
) : IRequest<IReadOnlyList<PostMediaResponse>>;
