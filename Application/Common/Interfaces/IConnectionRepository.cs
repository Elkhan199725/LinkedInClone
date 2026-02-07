using Domain.Entities;

namespace Application.Common.Interfaces;

public interface IConnectionRepository : IBaseRepository<Connection>
{
    Task<bool> AreConnectedAsync(Guid userId1, Guid userId2, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Connection>> GetConnectionsAsync(Guid userId, int page, int pageSize, CancellationToken cancellationToken = default);
    Task<int> GetConnectionsCountAsync(Guid userId, CancellationToken cancellationToken = default);
    Task DeleteConnectionAsync(Guid userId1, Guid userId2, CancellationToken cancellationToken = default);
}
