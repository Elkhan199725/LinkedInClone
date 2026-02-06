using Application.Account.Commands;
using Application.Common.Exceptions;
using Domain.Entities;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;

namespace Application.Account.Handlers;

public sealed class ChangePasswordCommandHandler : IRequestHandler<ChangePasswordCommand, bool>
{
    private readonly UserManager<AppUser> _userManager;
    private readonly ILogger<ChangePasswordCommandHandler> _logger;

    public ChangePasswordCommandHandler(
        UserManager<AppUser> userManager,
        ILogger<ChangePasswordCommandHandler> logger)
    {
        _userManager = userManager;
        _logger = logger;
    }

    public async Task<bool> Handle(ChangePasswordCommand command, CancellationToken cancellationToken)
    {
        _logger.LogDebug("Processing password change for user {UserId}", command.UserId);

        var user = await _userManager.FindByIdAsync(command.UserId.ToString());
        if (user is null)
        {
            _logger.LogWarning("Change password failed: user {UserId} not found", command.UserId);
            throw new NotFoundException("User", command.UserId);
        }

        var result = await _userManager.ChangePasswordAsync(
            user,
            command.Request.CurrentPassword,
            command.Request.NewPassword);

        if (!result.Succeeded)
        {
            var errors = result.Errors.Select(e => e.Description).ToList();
            _logger.LogWarning("Change password failed for user {UserId}: {Errors}", command.UserId, string.Join("; ", errors));

            if (errors.Any(e => e.Contains("Incorrect password", StringComparison.OrdinalIgnoreCase)))
            {
                throw new AuthenticationException("Current password is incorrect.");
            }

            throw new InvalidOperationException($"Failed to change password: {string.Join("; ", errors)}");
        }

        user.UpdatedAt = DateTime.UtcNow;
        await _userManager.UpdateAsync(user);

        _logger.LogInformation("Password successfully changed for user {UserId}", command.UserId);
        return true;
    }
}
