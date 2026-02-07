using Application.Common.Interfaces;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Repositories;

public sealed class UserFollowRepository : BaseRepository<UserFollow>, IUserFollowRepository
{
    public UserFollowRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<UserFollow?> GetFollowAsync(Guid followerId, Guid followedUserId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .FirstOrDefaultAsync(f => f.FollowerId == followerId && f.FollowedUserId == followedUserId, cancellationToken);
    }

    public async Task<IReadOnlyList<UserFollow>> GetFollowingAsync(Guid userId, int page, int pageSize, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(f => f.FollowerId == userId)
            .Include(f => f.FollowedUser)
            .OrderByDescending(f => f.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<UserFollow>> GetFollowersAsync(Guid userId, int page, int pageSize, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(f => f.FollowedUserId == userId)
            .Include(f => f.Follower)
            .OrderByDescending(f => f.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);
    }

    public async Task<int> GetFollowingCountAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await _dbSet.CountAsync(f => f.FollowerId == userId, cancellationToken);
    }

    public async Task<int> GetFollowersCountAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await _dbSet.CountAsync(f => f.FollowedUserId == userId, cancellationToken);
    }

    public async Task DeleteMutualFollowsAsync(Guid userId1, Guid userId2, CancellationToken cancellationToken = default)
    {
        await _dbSet
            .Where(f => (f.FollowerId == userId1 && f.FollowedUserId == userId2) ||
                        (f.FollowerId == userId2 && f.FollowedUserId == userId1))
            .ExecuteDeleteAsync(cancellationToken);
    }
}
