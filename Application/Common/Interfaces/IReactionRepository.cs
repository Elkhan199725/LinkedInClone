using Domain.Entities;

namespace Application.Common.Interfaces;

public interface IReactionRepository : IBaseRepository<Reaction>
{
    Task<Reaction?> GetUserReactionAsync(Guid postId, Guid userId, CancellationToken cancellationToken = default);
}
