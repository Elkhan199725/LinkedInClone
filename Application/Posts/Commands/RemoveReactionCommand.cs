using MediatR;

namespace Application.Posts.Commands;

public sealed record RemoveReactionCommand(
    Guid PostId,
    Guid UserId
) : IRequest<bool>;
