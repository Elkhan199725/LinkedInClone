using Application.Profiles.Dtos;
using MediatR;

namespace Application.Profiles.Queries;

public sealed record GetMyProfileQuery(Guid AppUserId) : IRequest<MyProfileResponse>;
