using Application.Common.Exceptions;
using Application.Common.Interfaces;
using Application.Posts.Commands;
using Domain.Entities;
using MediatR;

namespace Application.Posts.Handlers;

public sealed class RemoveReactionCommandHandler : IRequestHandler<RemoveReactionCommand, bool>
{
    private readonly IReactionRepository _reactionRepository;

    public RemoveReactionCommandHandler(IReactionRepository reactionRepository)
    {
        _reactionRepository = reactionRepository;
    }

    public async Task<bool> Handle(RemoveReactionCommand command, CancellationToken cancellationToken)
    {
        var reaction = await _reactionRepository.GetUserReactionAsync(
            command.PostId,
            command.UserId,
            cancellationToken);

        if (reaction is null)
            throw new NotFoundException(nameof(Reaction), $"PostId: {command.PostId}, UserId: {command.UserId}");

        _reactionRepository.Remove(reaction);
        await _reactionRepository.SaveChangesAsync(cancellationToken);

        return true;
    }
}
