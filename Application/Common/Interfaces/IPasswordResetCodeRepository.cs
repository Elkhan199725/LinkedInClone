using Domain.Entities;

namespace Application.Common.Interfaces;

public interface IPasswordResetCodeRepository : IBaseRepository<PasswordResetCode>
{
    Task<PasswordResetCode?> GetActiveByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
    Task InvalidateAllForUserAsync(Guid userId, CancellationToken cancellationToken = default);
}
