using Domain.Entities;
using Domain.Enums;

namespace Application.Common.Interfaces;

public interface IConnectionRequestRepository : IBaseRepository<ConnectionRequest>
{
    Task<ConnectionRequest?> GetPendingBetweenUsersAsync(Guid userId1, Guid userId2, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<ConnectionRequest>> GetIncomingRequestsAsync(Guid userId, int page, int pageSize, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<ConnectionRequest>> GetOutgoingRequestsAsync(Guid userId, int page, int pageSize, CancellationToken cancellationToken = default);
    Task<ConnectionRequest?> GetByIdWithUsersAsync(Guid requestId, CancellationToken cancellationToken = default);
}
