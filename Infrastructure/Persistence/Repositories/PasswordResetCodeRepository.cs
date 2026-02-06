using Application.Common.Interfaces;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Repositories;

public sealed class PasswordResetCodeRepository : BaseRepository<PasswordResetCode>, IPasswordResetCodeRepository
{
    public PasswordResetCodeRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<PasswordResetCode?> GetActiveByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(x => x.UserId == userId && !x.IsUsed && x.ExpiresAtUtc > DateTime.UtcNow)
            .OrderByDescending(x => x.CreatedAt)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task InvalidateAllForUserAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        await _dbSet
            .Where(x => x.UserId == userId && !x.IsUsed)
            .ExecuteUpdateAsync(x => x.SetProperty(p => p.IsUsed, true), cancellationToken);
    }
}
