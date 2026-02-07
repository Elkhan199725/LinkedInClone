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
    private readonly IPostMediaRepository _postMediaRepository;
    private readonly IUserProfileRepository _userProfileRepository;
    private readonly IMapper _mapper;

    public UpdatePostCommandHandler(
        IPostRepository postRepository,
        IPostMediaRepository postMediaRepository,
        IUserProfileRepository userProfileRepository,
        IMapper mapper)
    {
        _postRepository = postRepository;
        _postMediaRepository = postMediaRepository;
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

        if (command.Request.Media is not null)
        {
            var existingMedia = await _postMediaRepository.GetByPostIdAsync(command.PostId, cancellationToken);

            if (existingMedia.Count > 0)
            {
                await _postMediaRepository.RemoveRangeAsync(existingMedia, cancellationToken);
            }

            if (command.Request.Media.Count > 0)
            {
                var newMedia = command.Request.Media.Select((m, index) => new PostMedia
                {
                    PostId = command.PostId,
                    Type = m.Type,
                    Url = m.Url,
                    PublicId = m.PublicId,
                    Order = m.Order > 0 ? m.Order : index,
                    Width = m.Width,
                    Height = m.Height,
                    Duration = m.Duration,
                    CreatedAt = DateTime.UtcNow
                }).ToList();

                foreach (var media in newMedia)
                {
                    await _postMediaRepository.AddAsync(media, cancellationToken);
                }
            }
        }

        _postRepository.Update(post);
        await _postRepository.SaveChangesAsync(cancellationToken);

        var updatedPost = await _postRepository.GetPostWithDetailsAsync(command.PostId, cancellationToken);
        var authorProfile = await _userProfileRepository.GetByIdAsync(post.AuthorId, cancellationToken);

        var mediaResponses = updatedPost!.Media
            .OrderBy(m => m.Order)
            .Select(m => _mapper.Map<PostMediaResponse>(m))
            .ToList();

        return new PostResponse(
            Id: updatedPost.Id,
            AuthorId: updatedPost.AuthorId,
            AuthorName: authorProfile != null ? $"{authorProfile.FirstName} {authorProfile.LastName}" : "Unknown",
            AuthorProfilePhotoUrl: authorProfile?.ProfilePhotoUrl,
            Text: updatedPost.Text,
            Visibility: updatedPost.Visibility,
            Media: mediaResponses,
            ReactionsCount: updatedPost.Reactions.Count,
            CommentsCount: updatedPost.Comments.Count,
            CurrentUserReaction: updatedPost.Reactions.FirstOrDefault(r => r.UserId == command.UserId)?.Type,
            CreatedAt: updatedPost.CreatedAt,
            UpdatedAt: updatedPost.UpdatedAt
        );
    }
}
