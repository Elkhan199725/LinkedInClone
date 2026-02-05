using Domain.Entities;

namespace Application.Common.Interfaces;

public interface IReactionRepository : IBaseRepository<Reaction>
{
    /// <summary>
    /// Gets a user's reaction to a specific post.
    /// </summary>
    Task<Reaction?> GetUserReactionAsync(Guid postId, Guid userId, CancellationToken cancellationToken = default);
}
