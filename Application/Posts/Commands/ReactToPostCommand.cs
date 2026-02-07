using Application.Posts.Dtos;
using Domain.Enums;
using MediatR;

namespace Application.Posts.Commands;

public sealed record ReactToPostCommand(
    Guid PostId,
    Guid UserId,
    ReactionType Type
) : IRequest<ReactionResponse?>;
