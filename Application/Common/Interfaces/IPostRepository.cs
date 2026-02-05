using Domain.Entities;

namespace Application.Common.Interfaces;

public interface IPostRepository : IBaseRepository<Post>
{
    /// <summary>
    /// Gets a post with all related data (media, reactions count, comments count).
    /// </summary>
    Task<Post?> GetPostWithDetailsAsync(Guid postId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets posts by author with pagination.
    /// </summary>
    Task<IReadOnlyList<Post>> GetPostsByAuthorAsync(Guid authorId, int page, int pageSize, CancellationToken cancellationToken = default);
}
