using Application.Common.Interfaces;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Repositories;

public sealed class ConnectionRepository : BaseRepository<Connection>, IConnectionRepository
{
    public ConnectionRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<bool> AreConnectedAsync(Guid userId1, Guid userId2, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .AnyAsync(c => c.UserId == userId1 && c.ConnectedUserId == userId2, cancellationToken);
    }

    public async Task<IReadOnlyList<Connection>> GetConnectionsAsync(Guid userId, int page, int pageSize, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(c => c.UserId == userId)
            .Include(c => c.ConnectedUser)
            .OrderByDescending(c => c.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);
    }

    public async Task<int> GetConnectionsCountAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await _dbSet.CountAsync(c => c.UserId == userId, cancellationToken);
    }

    public async Task DeleteConnectionAsync(Guid userId1, Guid userId2, CancellationToken cancellationToken = default)
    {
        await _dbSet
            .Where(c => (c.UserId == userId1 && c.ConnectedUserId == userId2) ||
                        (c.UserId == userId2 && c.ConnectedUserId == userId1))
            .ExecuteDeleteAsync(cancellationToken);
    }
}
