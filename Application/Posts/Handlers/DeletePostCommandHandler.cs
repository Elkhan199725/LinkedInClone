using Application.Common.Exceptions;
using Application.Common.Interfaces;
using Application.Posts.Commands;
using Domain.Entities;
using MediatR;

namespace Application.Posts.Handlers;

public sealed class DeletePostCommandHandler : IRequestHandler<DeletePostCommand, bool>
{
    private readonly IPostRepository _postRepository;

    public DeletePostCommandHandler(IPostRepository postRepository)
    {
        _postRepository = postRepository;
    }

    public async Task<bool> Handle(DeletePostCommand command, CancellationToken cancellationToken)
    {
        var post = await _postRepository.GetByIdAsync(command.PostId, cancellationToken);

        if (post is null)
            throw new NotFoundException(nameof(Post), command.PostId);

        if (post.AuthorId != command.UserId)
            throw new ForbiddenException("You can only delete your own posts.");

        _postRepository.Remove(post);
        await _postRepository.SaveChangesAsync(cancellationToken);

        return true;
    }
}
