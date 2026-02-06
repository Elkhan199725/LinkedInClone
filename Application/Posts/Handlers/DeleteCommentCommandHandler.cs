using Application.Common.Exceptions;
using Application.Common.Interfaces;
using Application.Posts.Commands;
using Domain.Entities;
using MediatR;

namespace Application.Posts.Handlers;

public sealed class DeleteCommentCommandHandler : IRequestHandler<DeleteCommentCommand, bool>
{
    private readonly ICommentRepository _commentRepository;

    public DeleteCommentCommandHandler(ICommentRepository commentRepository)
    {
        _commentRepository = commentRepository;
    }

    public async Task<bool> Handle(DeleteCommentCommand command, CancellationToken cancellationToken)
    {
        var comment = await _commentRepository.GetByIdAsync(command.CommentId, cancellationToken);

        if (comment is null)
            throw new NotFoundException(nameof(Comment), command.CommentId);

        if (comment.PostId != command.PostId)
            throw new NotFoundException(nameof(Comment), command.CommentId);

        if (comment.AuthorId != command.UserId)
            throw new ForbiddenException("You can only delete your own comments.");

        if (comment.ParentCommentId is null)
        {
            var replies = await _commentRepository.GetRepliesByParentIdAsync(comment.Id, cancellationToken);
            if (replies.Count > 0)
            {
                await _commentRepository.RemoveRangeAsync(replies, cancellationToken);
            }
        }

        _commentRepository.Remove(comment);
        await _commentRepository.SaveChangesAsync(cancellationToken);

        return true;
    }
}
