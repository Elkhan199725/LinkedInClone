using Application.Common.Interfaces;
using Application.Posts.Dtos;
using Application.Posts.Queries;
using Domain.Entities;
using MediatR;

namespace Application.Posts.Handlers;

public sealed class GetPostCommentsQueryHandler : IRequestHandler<GetPostCommentsQuery, IReadOnlyList<CommentResponse>>
{
    private readonly ICommentRepository _commentRepository;
    private readonly IUserProfileRepository _userProfileRepository;

    public GetPostCommentsQueryHandler(
        ICommentRepository commentRepository,
        IUserProfileRepository userProfileRepository)
    {
        _commentRepository = commentRepository;
        _userProfileRepository = userProfileRepository;
    }

    public async Task<IReadOnlyList<CommentResponse>> Handle(GetPostCommentsQuery request, CancellationToken cancellationToken)
    {
        var comments = await _commentRepository.GetCommentsByPostAsync(
            request.PostId,
            request.Page,
            request.PageSize,
            cancellationToken);

        var authorIds = comments
            .SelectMany(c => new[] { c.AuthorId }.Concat(c.Replies.Select(r => r.AuthorId)))
            .Distinct()
            .ToList();

        var authorProfiles = new Dictionary<Guid, (string Name, string? PhotoUrl)>();
        foreach (var authorId in authorIds)
        {
            var profile = await _userProfileRepository.GetByIdAsync(authorId, cancellationToken);
            if (profile != null)
            {
                authorProfiles[authorId] = ($"{profile.FirstName} {profile.LastName}", profile.ProfilePhotoUrl);
            }
        }

        return comments.Select(c => MapComment(c, authorProfiles)).ToList();
    }

    private static CommentResponse MapComment(
        Comment comment,
        Dictionary<Guid, (string Name, string? PhotoUrl)> authorProfiles)
    {
        var (authorName, authorPhotoUrl) = authorProfiles.GetValueOrDefault(
            comment.AuthorId,
            ("Unknown", null));

        var replies = comment.Replies
            .OrderBy(r => r.CreatedAt)
            .Select(r => MapComment(r, authorProfiles))
            .ToList();

        return new CommentResponse(
            Id: comment.Id,
            PostId: comment.PostId,
            AuthorId: comment.AuthorId,
            AuthorName: authorName,
            AuthorProfilePhotoUrl: authorPhotoUrl,
            Text: comment.Text,
            ParentCommentId: comment.ParentCommentId,
            Replies: replies,
            CreatedAt: comment.CreatedAt,
            UpdatedAt: comment.UpdatedAt
        );
    }
}
