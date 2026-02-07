using Domain.Entities;

namespace Application.Common.Interfaces;

public interface IUserFollowRepository : IBaseRepository<UserFollow>
{
    Task<UserFollow?> GetFollowAsync(Guid followerId, Guid followedUserId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<UserFollow>> GetFollowingAsync(Guid userId, int page, int pageSize, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<UserFollow>> GetFollowersAsync(Guid userId, int page, int pageSize, CancellationToken cancellationToken = default);
    Task<int> GetFollowingCountAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<int> GetFollowersCountAsync(Guid userId, CancellationToken cancellationToken = default);
    Task DeleteMutualFollowsAsync(Guid userId1, Guid userId2, CancellationToken cancellationToken = default);
}
