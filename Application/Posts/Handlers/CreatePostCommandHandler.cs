using Application.Common.Interfaces;
using Application.Posts.Commands;
using Application.Posts.Dtos;
using Domain.Entities;
using MediatR;

namespace Application.Posts.Handlers;

public sealed class CreatePostCommandHandler : IRequestHandler<CreatePostCommand, PostResponse>
{
    private readonly IPostRepository _postRepository;
    private readonly IUserProfileRepository _userProfileRepository;

    public CreatePostCommandHandler(
        IPostRepository postRepository,
        IUserProfileRepository userProfileRepository)
    {
        _postRepository = postRepository;
        _userProfileRepository = userProfileRepository;
    }

    public async Task<PostResponse> Handle(CreatePostCommand command, CancellationToken cancellationToken)
    {
        var request = command.Request;

        // Get author profile for response
        var authorProfile = await _userProfileRepository.GetByIdAsync(command.AuthorId, cancellationToken);

        var post = new Post
        {
            AuthorId = command.AuthorId,
            Text = request.Text?.Trim(),
            Visibility = request.Visibility,
            CreatedAt = DateTime.UtcNow
        };

        await _postRepository.AddAsync(post, cancellationToken);
        await _postRepository.SaveChangesAsync(cancellationToken);

        return new PostResponse(
            Id: post.Id,
            AuthorId: post.AuthorId,
            AuthorName: authorProfile != null ? $"{authorProfile.FirstName} {authorProfile.LastName}" : "Unknown",
            AuthorProfilePhotoUrl: authorProfile?.ProfilePhotoUrl,
            Text: post.Text,
            Visibility: post.Visibility,
            Media: [],
            ReactionsCount: 0,
            CommentsCount: 0,
            CurrentUserReaction: null,
            CreatedAt: post.CreatedAt,
            UpdatedAt: post.UpdatedAt
        );
    }
}
