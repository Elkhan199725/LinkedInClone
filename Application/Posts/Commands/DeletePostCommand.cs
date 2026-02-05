using MediatR;

namespace Application.Posts.Commands;

public sealed record DeletePostCommand(
    Guid PostId,
    Guid UserId
) : IRequest<bool>;
