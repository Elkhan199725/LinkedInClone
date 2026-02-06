using Application.Profiles.Dtos;
using AutoMapper;
using Domain.Entities;

namespace Application.Profiles.Mapping;

public sealed class ProfileMappingProfile : Profile
{
    public ProfileMappingProfile()
    {
        CreateMap<UserProfile, MyProfileResponse>();

        CreateMap<UserProfile, PublicProfileResponse>();

        CreateMap<UpdateMyProfileRequest, UserProfile>()
            .ForMember(dest => dest.AppUserId, opt => opt.Ignore())
            .ForMember(dest => dest.AppUser, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore());
    }
}
