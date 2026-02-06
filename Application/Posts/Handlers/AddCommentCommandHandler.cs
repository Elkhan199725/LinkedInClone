using Application.Common.Exceptions;
using Application.Common.Interfaces;
using Application.Posts.Commands;
using Application.Posts.Dtos;
using Domain.Entities;
using MediatR;

namespace Application.Posts.Handlers;

public sealed class AddCommentCommandHandler : IRequestHandler<AddCommentCommand, CommentResponse>
{
    private readonly IPostRepository _postRepository;
    private readonly ICommentRepository _commentRepository;
    private readonly IUserProfileRepository _userProfileRepository;

    public AddCommentCommandHandler(
        IPostRepository postRepository,
        ICommentRepository commentRepository,
        IUserProfileRepository userProfileRepository)
    {
        _postRepository = postRepository;
        _commentRepository = commentRepository;
        _userProfileRepository = userProfileRepository;
    }

    public async Task<CommentResponse> Handle(AddCommentCommand command, CancellationToken cancellationToken)
    {
        var request = command.Request;

        var post = await _postRepository.GetByIdAsync(command.PostId, cancellationToken);
        if (post is null)
            throw new NotFoundException(nameof(Post), command.PostId);

        Guid? parentId = null;

        if (request.ParentCommentId.HasValue)
        {
            var parentComment = await _commentRepository
                .GetByIdAsync(request.ParentCommentId.Value, cancellationToken);

            if (parentComment is null)
                throw new NotFoundException(nameof(Comment), request.ParentCommentId.Value);

            if (parentComment.PostId != command.PostId)
                throw new ForbiddenException("Parent comment does not belong to this post.");

            // Keep comments to 2 levels max:
            // If replying to a reply, attach to the top-level comment
            parentId = parentComment.ParentCommentId ?? parentComment.Id;
        }

        var authorProfile = await _userProfileRepository
            .GetByIdAsync(command.AuthorId, cancellationToken);

        var comment = new Comment
        {
            PostId = command.PostId,
            AuthorId = command.AuthorId,
            ParentCommentId = parentId,
            Text = request.Text.Trim(),
            CreatedAt = DateTime.UtcNow
        };

        await _commentRepository.AddAsync(comment, cancellationToken);
        await _commentRepository.SaveChangesAsync(cancellationToken);

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
