using Application.Common.Interfaces;
using Domain.Entities;
using Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Repositories;

public sealed class ConnectionRequestRepository : BaseRepository<ConnectionRequest>, IConnectionRequestRepository
{
    public ConnectionRequestRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<ConnectionRequest?> GetPendingBetweenUsersAsync(Guid userId1, Guid userId2, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .FirstOrDefaultAsync(r =>
                r.Status == ConnectionRequestStatus.Pending &&
                ((r.SenderId == userId1 && r.ReceiverId == userId2) ||
                 (r.SenderId == userId2 && r.ReceiverId == userId1)),
                cancellationToken);
    }

    public async Task<IReadOnlyList<ConnectionRequest>> GetIncomingRequestsAsync(Guid userId, int page, int pageSize, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(r => r.ReceiverId == userId && r.Status == ConnectionRequestStatus.Pending)
            .Include(r => r.Sender)
            .OrderByDescending(r => r.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<ConnectionRequest>> GetOutgoingRequestsAsync(Guid userId, int page, int pageSize, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(r => r.SenderId == userId && r.Status == ConnectionRequestStatus.Pending)
            .Include(r => r.Receiver)
            .OrderByDescending(r => r.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);
    }

    public async Task<ConnectionRequest?> GetByIdWithUsersAsync(Guid requestId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(r => r.Sender)
            .Include(r => r.Receiver)
            .FirstOrDefaultAsync(r => r.Id == requestId, cancellationToken);
    }
}
