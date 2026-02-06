namespace Application.Common.Interfaces;

public interface IAccountDeletionService
{
    Task DeleteAccountAsync(Guid userId, CancellationToken cancellationToken = default);
}
