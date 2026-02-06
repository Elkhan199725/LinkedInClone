using Application.Common.Exceptions;
using Application.Common.Interfaces;
using Application.Posts.Commands;
using Application.Posts.Dtos;
using AutoMapper;
using Domain.Entities;
using MediatR;

namespace Application.Posts.Handlers;

public sealed class AddPostMediaCommandHandler : IRequestHandler<AddPostMediaCommand, IReadOnlyList<PostMediaResponse>>
{
    private readonly IPostRepository _postRepository;
    private readonly IPostMediaRepository _postMediaRepository;
    private readonly IMapper _mapper;

    public AddPostMediaCommandHandler(
        IPostRepository postRepository,
        IPostMediaRepository postMediaRepository,
        IMapper mapper)
    {
        _postRepository = postRepository;
        _postMediaRepository = postMediaRepository;
        _mapper = mapper;
    }

    public async Task<IReadOnlyList<PostMediaResponse>> Handle(AddPostMediaCommand command, CancellationToken cancellationToken)
    {
        var post = await _postRepository.GetByIdAsync(command.PostId, cancellationToken);

        if (post is null)
            throw new NotFoundException(nameof(Post), command.PostId);

        if (post.AuthorId != command.UserId)
            throw new ForbiddenException("You can only add media to your own posts.");

        var mediaItems = new List<PostMedia>();

        foreach (var item in command.MediaItems)
        {
            var media = new PostMedia
            {
                PostId = command.PostId,
                Type = item.Type,
                Url = item.Url,
                PublicId = item.PublicId,
                Order = item.Order,
                Width = item.Width,
                Height = item.Height,
                Duration = item.Duration,
                CreatedAt = DateTime.UtcNow
            };

            await _postMediaRepository.AddAsync(media, cancellationToken);
            mediaItems.Add(media);
        }

        await _postMediaRepository.SaveChangesAsync(cancellationToken);

        return mediaItems.Select(m => _mapper.Map<PostMediaResponse>(m)).ToList();
    }
}
