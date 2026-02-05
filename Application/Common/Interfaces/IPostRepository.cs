using Domain.Entities;

namespace Application.Common.Interfaces;

public interface IPostRepository : IBaseRepository<Post>
{
    Task<Post?> GetPostWithDetailsAsync(Guid postId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Post>> GetPostsByAuthorAsync(Guid authorId, int page, int pageSize, CancellationToken cancellationToken = default);
}
