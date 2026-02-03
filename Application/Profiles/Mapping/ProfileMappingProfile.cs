using Application.Profiles.Dtos;
using AutoMapper;
using Domain.Entities;

namespace Application.Profiles.Mapping;

public sealed class ProfileMappingProfile : Profile
{
    public ProfileMappingProfile()
    {
        // UserProfile -> MyProfileResponse
        CreateMap<UserProfile, MyProfileResponse>();

        // UserProfile -> PublicProfileResponse
        CreateMap<UserProfile, PublicProfileResponse>();

        // UpdateMyProfileRequest -> UserProfile
        // Note: AppUserId, CreatedAt, UpdatedAt are NOT mapped here
        CreateMap<UpdateMyProfileRequest, UserProfile>()
            .ForMember(dest => dest.AppUserId, opt => opt.Ignore())
            .ForMember(dest => dest.AppUser, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore());
    }
}
