using Application.Common.Exceptions;
using Application.Common.Interfaces;
using Application.Posts.Dtos;
using Application.Posts.Queries;
using AutoMapper;
using Domain.Entities;
using MediatR;

namespace Application.Posts.Handlers;

public sealed class GetPostByIdQueryHandler : IRequestHandler<GetPostByIdQuery, PostResponse>
{
    private readonly IPostRepository _postRepository;
    private readonly IUserProfileRepository _userProfileRepository;
    private readonly IReactionRepository _reactionRepository;
    private readonly IMapper _mapper;

    public GetPostByIdQueryHandler(
        IPostRepository postRepository,
        IUserProfileRepository userProfileRepository,
        IReactionRepository reactionRepository,
        IMapper mapper)
    {
        _postRepository = postRepository;
        _userProfileRepository = userProfileRepository;
        _reactionRepository = reactionRepository;
        _mapper = mapper;
    }

    public async Task<PostResponse> Handle(GetPostByIdQuery request, CancellationToken cancellationToken)
    {
        var post = await _postRepository.GetPostWithDetailsAsync(request.PostId, cancellationToken);

        if (post is null)
            throw new NotFoundException(nameof(Post), request.PostId);

        var authorProfile = await _userProfileRepository.GetByIdAsync(post.AuthorId, cancellationToken);

        Reaction? currentUserReaction = null;
        if (request.CurrentUserId.HasValue)
        {
            currentUserReaction = await _reactionRepository.GetUserReactionAsync(
                request.PostId,
                request.CurrentUserId.Value,
                cancellationToken);
        }

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
            CurrentUserReaction: currentUserReaction?.Type,
            CreatedAt: post.CreatedAt,
            UpdatedAt: post.UpdatedAt
        );
    }
}
