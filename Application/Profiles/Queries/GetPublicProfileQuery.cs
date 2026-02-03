using Application.Profiles.Dtos;
using MediatR;

namespace Application.Profiles.Queries;

public sealed record GetPublicProfileQuery(Guid AppUserId) : IRequest<PublicProfileResponse>;
