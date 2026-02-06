using Application.Common.Exceptions;
using Application.Common.Interfaces;
using Application.Posts.Commands;
using Application.Posts.Dtos;
using Domain.Entities;
using MediatR;

namespace Application.Posts.Handlers;

public sealed class UpdateCommentCommandHandler : IRequestHandler<UpdateCommentCommand, CommentResponse>
{
    private readonly ICommentRepository _commentRepository;
    private readonly IUserProfileRepository _userProfileRepository;

    public UpdateCommentCommandHandler(
        ICommentRepository commentRepository,
        IUserProfileRepository userProfileRepository)
    {
        _commentRepository = commentRepository;
        _userProfileRepository = userProfileRepository;
    }

    public async Task<CommentResponse> Handle(UpdateCommentCommand command, CancellationToken cancellationToken)
    {
        var comment = await _commentRepository.GetByIdAsync(command.CommentId, cancellationToken);

        if (comment is null)
            throw new NotFoundException(nameof(Comment), command.CommentId);

        if (comment.PostId != command.PostId)
            throw new NotFoundException(nameof(Comment), command.CommentId);

        if (comment.AuthorId != command.UserId)
            throw new ForbiddenException("You can only edit your own comments.");

        comment.Text = command.Request.Text.Trim();
        comment.UpdatedAt = DateTime.UtcNow;

        _commentRepository.Update(comment);
        await _commentRepository.SaveChangesAsync(cancellationToken);

        var authorProfile = await _userProfileRepository.GetByIdAsync(comment.AuthorId, cancellationToken);

        return new CommentResponse(
            Id: comment.Id,
            PostId: comment.PostId,
            AuthorId: comment.AuthorId,
            AuthorName: authorProfile != null
                ? $"{authorProfile.FirstName} {authorProfile.LastName}"
                : "Unknown",
            AuthorProfilePhotoUrl: authorProfile?.ProfilePhotoUrl,
            Text: comment.Text,
            ParentCommentId: comment.ParentCommentId,
            Replies: [],
            CreatedAt: comment.CreatedAt,
            UpdatedAt: comment.UpdatedAt
        );
    }
}
