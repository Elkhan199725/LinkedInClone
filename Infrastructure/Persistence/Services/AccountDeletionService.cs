using Application.Common.Interfaces;
using Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Persistence.Services;

public sealed class AccountDeletionService : IAccountDeletionService
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<AccountDeletionService> _logger;

    public AccountDeletionService(
        ApplicationDbContext context,
        ILogger<AccountDeletionService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task DeleteAccountAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Starting account deletion for user {UserId}", userId);

        var strategy = _context.Database.CreateExecutionStrategy();

        await strategy.ExecuteAsync(async () =>
        {
            await using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);

            try
            {
                _logger.LogDebug("Step 1: Fetching user posts for {UserId}", userId);
                var userPostIds = await _context.Posts
                    .Where(p => p.AuthorId == userId)
                    .Select(p => p.Id)
                    .ToListAsync(cancellationToken);

                _logger.LogDebug("Found {Count} posts for user {UserId}", userPostIds.Count, userId);

                if (userPostIds.Count > 0)
                {
                    _logger.LogDebug("Step 2a: Deleting reactions on user's posts");
                    var reactionsDeleted = await _context.Reactions
                        .Where(r => userPostIds.Contains(r.PostId))
                        .ExecuteDeleteAsync(cancellationToken);
                    _logger.LogDebug("Deleted {Count} reactions on user's posts", reactionsDeleted);

                    _logger.LogDebug("Step 2b: Deleting reply comments on user's posts");
                    var repliesDeleted = await _context.Comments
                        .Where(c => userPostIds.Contains(c.PostId) && c.ParentCommentId != null)
                        .ExecuteDeleteAsync(cancellationToken);
                    _logger.LogDebug("Deleted {Count} reply comments on user's posts", repliesDeleted);

                    _logger.LogDebug("Step 2c: Deleting top-level comments on user's posts");
                    var topCommentsDeleted = await _context.Comments
                        .Where(c => userPostIds.Contains(c.PostId) && c.ParentCommentId == null)
                        .ExecuteDeleteAsync(cancellationToken);
                    _logger.LogDebug("Deleted {Count} top-level comments on user's posts", topCommentsDeleted);

                    _logger.LogDebug("Step 2d: Deleting media on user's posts");
                    var mediaDeleted = await _context.PostMedia
                        .Where(m => userPostIds.Contains(m.PostId))
                        .ExecuteDeleteAsync(cancellationToken);
                    _logger.LogDebug("Deleted {Count} media items on user's posts", mediaDeleted);

                    _logger.LogDebug("Step 2e: Deleting user's posts");
                    var postsDeleted = await _context.Posts
                        .Where(p => p.AuthorId == userId)
                        .ExecuteDeleteAsync(cancellationToken);
                    _logger.LogDebug("Deleted {Count} posts", postsDeleted);
                }

                _logger.LogDebug("Step 3: Deleting user's reactions on other posts");
                var userReactionsDeleted = await _context.Reactions
                    .Where(r => r.UserId == userId)
                    .ExecuteDeleteAsync(cancellationToken);
                _logger.LogDebug("Deleted {Count} user reactions on other posts", userReactionsDeleted);

                _logger.LogDebug("Step 4a: Deleting user's reply comments on other posts");
                var userRepliesDeleted = await _context.Comments
                    .Where(c => c.AuthorId == userId && c.ParentCommentId != null)
                    .ExecuteDeleteAsync(cancellationToken);
                _logger.LogDebug("Deleted {Count} user reply comments", userRepliesDeleted);

                _logger.LogDebug("Step 4b: Finding user's top-level comments with replies");
                var userTopLevelCommentIds = await _context.Comments
                    .Where(c => c.AuthorId == userId && c.ParentCommentId == null)
                    .Select(c => c.Id)
                    .ToListAsync(cancellationToken);

                if (userTopLevelCommentIds.Count > 0)
                {
                    _logger.LogDebug("Step 4c: Deleting replies to user's {Count} top-level comments", userTopLevelCommentIds.Count);
                    var repliesToUserCommentsDeleted = await _context.Comments
                        .Where(c => c.ParentCommentId != null && userTopLevelCommentIds.Contains(c.ParentCommentId.Value))
                        .ExecuteDeleteAsync(cancellationToken);
                    _logger.LogDebug("Deleted {Count} replies to user's comments", repliesToUserCommentsDeleted);
                }

                _logger.LogDebug("Step 4d: Deleting user's top-level comments");
                var userTopCommentsDeleted = await _context.Comments
                    .Where(c => c.AuthorId == userId && c.ParentCommentId == null)
                    .ExecuteDeleteAsync(cancellationToken);
                _logger.LogDebug("Deleted {Count} user top-level comments", userTopCommentsDeleted);

                _logger.LogDebug("Step 5: Deleting user profile for {UserId}", userId);
                var profilesDeleted = await _context.UserProfiles
                    .Where(up => up.AppUserId == userId)
                    .ExecuteDeleteAsync(cancellationToken);
                _logger.LogDebug("Deleted {Count} user profiles", profilesDeleted);

                _logger.LogDebug("Step 6: Deleting AppUser for {UserId}", userId);
                var user = await _context.Users
                    .FirstOrDefaultAsync(u => u.Id == userId, cancellationToken);

                if (user is not null)
                {
                    _context.Users.Remove(user);
                    await _context.SaveChangesAsync(cancellationToken);
                    _logger.LogDebug("Deleted AppUser {UserId}", userId);
                }
                else
                {
                    _logger.LogWarning("AppUser {UserId} not found during deletion - may have been already deleted", userId);
                }

                await transaction.CommitAsync(cancellationToken);
                _logger.LogInformation("Successfully deleted account for user {UserId}", userId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to delete account for user {UserId}. Rolling back transaction.", userId);
                await transaction.RollbackAsync(cancellationToken);
                throw;
            }
        });
    }
}
