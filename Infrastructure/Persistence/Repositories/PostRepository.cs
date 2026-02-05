using Application.Common.Interfaces;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Repositories;

public sealed class PostRepository : BaseRepository<Post>, IPostRepository
{
    public PostRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<Post?> GetPostWithDetailsAsync(Guid postId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(p => p.Media.OrderBy(m => m.Order))
            .Include(p => p.Author)
            .Include(p => p.Reactions)
            .Include(p => p.Comments.Where(c => c.ParentCommentId == null))
                .ThenInclude(c => c.Author)
            .FirstOrDefaultAsync(p => p.Id == postId, cancellationToken);
    }

    public async Task<IReadOnlyList<Post>> GetPostsByAuthorAsync(
        Guid authorId,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(p => p.AuthorId == authorId)
            .OrderByDescending(p => p.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Include(p => p.Media.OrderBy(m => m.Order))
            .Include(p => p.Reactions)
            .ToListAsync(cancellationToken);
    }
}
