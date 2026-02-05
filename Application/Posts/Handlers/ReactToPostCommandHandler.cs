using Application.Common.Exceptions;
using Application.Common.Interfaces;
using Application.Posts.Commands;
using Application.Posts.Dtos;
using Domain.Entities;
using MediatR;

namespace Application.Posts.Handlers;

public sealed class ReactToPostCommandHandler : IRequestHandler<ReactToPostCommand, ReactionResponse>
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

    public async Task<ReactionResponse> Handle(ReactToPostCommand command, CancellationToken cancellationToken)
    {
        // Verify post exists
        var post = await _postRepository.GetByIdAsync(command.PostId, cancellationToken);

        if (post is null)
            throw new NotFoundException(nameof(Post), command.PostId);

        // Check if user already has a reaction on this post
        var existingReaction = await _reactionRepository.GetUserReactionAsync(
            command.PostId,
            command.UserId,
            cancellationToken);

        if (existingReaction is not null)
        {
            // Update existing reaction type
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

        // Create new reaction
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
