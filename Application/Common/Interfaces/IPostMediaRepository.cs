using Domain.Entities;

namespace Application.Common.Interfaces;

public interface IPostMediaRepository : IBaseRepository<PostMedia>
{
    Task<IReadOnlyList<PostMedia>> GetByPostIdAsync(Guid postId, CancellationToken cancellationToken = default);
    Task RemoveRangeAsync(IEnumerable<PostMedia> mediaItems, CancellationToken cancellationToken = default);
}
