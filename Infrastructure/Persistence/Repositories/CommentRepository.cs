using Application.Common.Interfaces;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Repositories;

public sealed class CommentRepository : BaseRepository<Comment>, ICommentRepository
{
    public CommentRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<IReadOnlyList<Comment>> GetCommentsByPostAsync(
        Guid postId,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(c => c.PostId == postId && c.ParentCommentId == null)
            .OrderByDescending(c => c.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Include(c => c.Author)
            .Include(c => c.Replies.OrderBy(r => r.CreatedAt))
                .ThenInclude(r => r.Author)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Comment>> GetRepliesByParentIdAsync(
        Guid parentCommentId,
        CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(c => c.ParentCommentId == parentCommentId)
            .ToListAsync(cancellationToken);
    }

    public Task RemoveRangeAsync(IEnumerable<Comment> comments, CancellationToken cancellationToken = default)
    {
        _dbSet.RemoveRange(comments);
        return Task.CompletedTask;
    }
}
