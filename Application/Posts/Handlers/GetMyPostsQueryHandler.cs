using Application.Common.Interfaces;
using Application.Posts.Dtos;
using Application.Posts.Queries;
using AutoMapper;
using MediatR;

namespace Application.Posts.Handlers;

public sealed class GetMyPostsQueryHandler : IRequestHandler<GetMyPostsQuery, PostListResponse>
{
    private readonly IPostRepository _postRepository;
    private readonly IUserProfileRepository _userProfileRepository;
    private readonly IMapper _mapper;

    public GetMyPostsQueryHandler(
        IPostRepository postRepository,
        IUserProfileRepository userProfileRepository,
        IMapper mapper)
    {
        _postRepository = postRepository;
        _userProfileRepository = userProfileRepository;
        _mapper = mapper;
    }

    public async Task<PostListResponse> Handle(GetMyPostsQuery request, CancellationToken cancellationToken)
    {
        var posts = await _postRepository.GetPostsByAuthorAsync(
            request.AuthorId,
            request.Page,
            request.PageSize,
            cancellationToken);

        // Get author profile (same for all posts since they're all by the same author)
        var authorProfile = await _userProfileRepository.GetByIdAsync(request.AuthorId, cancellationToken);
        var authorName = authorProfile != null ? $"{authorProfile.FirstName} {authorProfile.LastName}" : "Unknown";
        var authorPhotoUrl = authorProfile?.ProfilePhotoUrl;

        var postResponses = posts.Select(post => new PostResponse(
            Id: post.Id,
            AuthorId: post.AuthorId,
            AuthorName: authorName,
            AuthorProfilePhotoUrl: authorPhotoUrl,
            Text: post.Text,
            Visibility: post.Visibility,
            Media: post.Media
                .OrderBy(m => m.Order)
                .Select(m => _mapper.Map<PostMediaResponse>(m))
                .ToList(),
            ReactionsCount: post.Reactions.Count,
            CommentsCount: 0,
            CurrentUserReaction: post.Reactions.FirstOrDefault(r => r.UserId == request.AuthorId)?.Type,
            CreatedAt: post.CreatedAt,
            UpdatedAt: post.UpdatedAt
        )).ToList();

        return new PostListResponse(
            Posts: postResponses,
            Page: request.Page,
            PageSize: request.PageSize,
            HasMore: posts.Count == request.PageSize
        );
    }
}
