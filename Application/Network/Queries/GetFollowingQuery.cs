using Application.Network.Dtos;
using MediatR;

namespace Application.Network.Queries;

public sealed record GetFollowingQuery(Guid UserId, int Page = 1, int PageSize = 20) : IRequest<FollowListResponse>;
