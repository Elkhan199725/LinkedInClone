using Application.Common.Exceptions;
using Application.Common.Interfaces;
using Application.Posts.Commands;
using Application.Posts.Dtos;
using AutoMapper;
using Domain.Entities;
using MediatR;

namespace Application.Posts.Handlers;

public sealed class UpdatePostCommandHandler : IRequestHandler<UpdatePostCommand, PostResponse>
{
    private readonly IPostRepository _postRepository;
    private readonly IUserProfileRepository _userProfileRepository;
    private readonly IMapper _mapper;

    public UpdatePostCommandHandler(
        IPostRepository postRepository,
        IUserProfileRepository userProfileRepository,
        IMapper mapper)
    {
        _postRepository = postRepository;
        _userProfileRepository = userProfileRepository;
        _mapper = mapper;
    }

    public async Task<PostResponse> Handle(UpdatePostCommand command, CancellationToken cancellationToken)
    {
        var post = await _postRepository.GetPostWithDetailsAsync(command.PostId, cancellationToken);

        if (post is null)
            throw new NotFoundException(nameof(Post), command.PostId);

        if (post.AuthorId != command.UserId)
            throw new ForbiddenException("You can only edit your own posts.");

        post.Text = command.Request.Text?.Trim();
        post.Visibility = command.Request.Visibility;
        post.UpdatedAt = DateTime.UtcNow;

        _postRepository.Update(post);
        await _postRepository.SaveChangesAsync(cancellationToken);

        var authorProfile = await _userProfileRepository.GetByIdAsync(post.AuthorId, cancellationToken);

        var mediaResponses = post.Media
            .OrderBy(m => m.Order)
            .Select(m => _mapper.Map<PostMediaResponse>(m))
            .ToList();

        return new PostResponse(
            Id: post.Id,
            AuthorId: post.AuthorId,
            AuthorName: authorProfile != null ? $"{authorProfile.FirstName} {authorProfile.LastName}" : "Unknown",
            AuthorProfilePhotoUrl: authorProfile?.ProfilePhotoUrl,
            Text: post.Text,
            Visibility: post.Visibility,
            Media: mediaResponses,
            ReactionsCount: post.Reactions.Count,
            CommentsCount: post.Comments.Count,
            CurrentUserReaction: post.Reactions.FirstOrDefault(r => r.UserId == command.UserId)?.Type,
            CreatedAt: post.CreatedAt,
            UpdatedAt: post.UpdatedAt
        );
    }
}
