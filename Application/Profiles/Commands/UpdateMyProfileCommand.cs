using Application.Profiles.Dtos;
using MediatR;

namespace Application.Profiles.Commands;

public sealed record UpdateMyProfileCommand(
    Guid AppUserId,
    UpdateMyProfileRequest Request
) : IRequest<MyProfileResponse>;
