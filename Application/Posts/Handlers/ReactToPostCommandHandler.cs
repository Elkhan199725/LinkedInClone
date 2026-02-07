using Application.Common.Exceptions;
using Application.Common.Interfaces;
using Application.Posts.Commands;
using Application.Posts.Dtos;
using Domain.Entities;
using MediatR;

namespace Application.Posts.Handlers;

public sealed class ReactToPostCommandHandler : IRequestHandler<ReactToPostCommand, ReactionResponse?>
{
    private readonly IPostRepository _postRepository;
    private readonly IReactionRepository _reactionRepository;

    public ReactToPostCommandHandler(
        IPostRepository postRepository,
        IReactionRepository reactionRepository)
    {
        _postRepository = postRepository;
        _reactionRepository = reactionRepository;
    }

    public async Task<ReactionResponse?> Handle(ReactToPostCommand command, CancellationToken cancellationToken)
    {
        var post = await _postRepository.GetByIdAsync(command.PostId, cancellationToken);

        if (post is null)
            throw new NotFoundException(nameof(Post), command.PostId);

        var existingReaction = await _reactionRepository.GetUserReactionAsync(
            command.PostId,
            command.UserId,
            cancellationToken);

        if (existingReaction is not null)
        {
            if (existingReaction.Type == command.Type)
            {
                _reactionRepository.Remove(existingReaction);
                await _reactionRepository.SaveChangesAsync(cancellationToken);
                return null;
            }

            existingReaction.Type = command.Type;
            existingReaction.UpdatedAt = DateTime.UtcNow;

            _reactionRepository.Update(existingReaction);
            await _reactionRepository.SaveChangesAsync(cancellationToken);

            return new ReactionResponse(
                Id: existingReaction.Id,
                PostId: existingReaction.PostId,
                UserId: existingReaction.UserId,
                Type: existingReaction.Type,
                CreatedAt: existingReaction.CreatedAt
            );
        }

        var reaction = new Reaction
        {
            PostId = command.PostId,
            UserId = command.UserId,
            Type = command.Type,
            CreatedAt = DateTime.UtcNow
        };

        await _reactionRepository.AddAsync(reaction, cancellationToken);
        await _reactionRepository.SaveChangesAsync(cancellationToken);

        return new ReactionResponse(
            Id: reaction.Id,
            PostId: reaction.PostId,
            UserId: reaction.UserId,
            Type: reaction.Type,
            CreatedAt: reaction.CreatedAt
        );
    }
}
