using Application.Posts.Dtos;
using AutoMapper;
using Domain.Entities;

namespace Application.Posts.Mapping;

public sealed class PostMappingProfile : Profile
{
    public PostMappingProfile()
    {
        // PostMedia -> PostMediaResponse
        CreateMap<PostMedia, PostMediaResponse>();

        // Comment -> CommentResponse
        CreateMap<Comment, CommentResponse>()
            .ForMember(dest => dest.AuthorName, opt => opt.Ignore())
            .ForMember(dest => dest.AuthorProfilePhotoUrl, opt => opt.Ignore())
            .ForMember(dest => dest.Replies, opt => opt.MapFrom(src => src.Replies));

        // Reaction -> ReactionResponse
        CreateMap<Reaction, ReactionResponse>();

        // AddPostMediaRequest -> PostMedia
        CreateMap<AddPostMediaRequest, PostMedia>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.PostId, opt => opt.Ignore())
            .ForMember(dest => dest.Post, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore());
    }
}
