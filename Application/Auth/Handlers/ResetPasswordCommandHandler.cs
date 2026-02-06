using Application.Auth.Commands;
using Application.Common.Exceptions;
using Application.Common.Interfaces;
using Domain.Entities;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using System.Security.Cryptography;
using System.Text;

namespace Application.Auth.Handlers;

public sealed class ResetPasswordCommandHandler : IRequestHandler<ResetPasswordCommand, bool>
{
    private readonly UserManager<AppUser> _userManager;
    private readonly IPasswordResetCodeRepository _resetCodeRepository;
    private readonly ILogger<ResetPasswordCommandHandler> _logger;

    private const int MaxAttempts = 5;

    public ResetPasswordCommandHandler(
        UserManager<AppUser> userManager,
        IPasswordResetCodeRepository resetCodeRepository,
        ILogger<ResetPasswordCommandHandler> logger)
    {
        _userManager = userManager;
        _resetCodeRepository = resetCodeRepository;
        _logger = logger;
    }

    public async Task<bool> Handle(ResetPasswordCommand command, CancellationToken cancellationToken)
    {
        var email = command.Request.Email.Trim().ToLowerInvariant();
        var code = command.Request.Code;
        var newPassword = command.Request.NewPassword;

        _logger.LogDebug("Processing password reset request for email: {Email}", email);

        var user = await _userManager.FindByEmailAsync(email);
        if (user is null)
        {
            _logger.LogWarning("Password reset failed: user not found for email {Email}", email);
            throw new AuthenticationException("Invalid or expired reset code.");
        }

        var resetCode = await _resetCodeRepository.GetActiveByUserIdAsync(user.Id, cancellationToken);
        if (resetCode is null)
        {
            _logger.LogWarning("Password reset failed: no active reset code for user {UserId}", user.Id);
            throw new AuthenticationException("Invalid or expired reset code.");
        }

        if (resetCode.Attempts >= MaxAttempts)
        {
            _logger.LogWarning("Password reset failed: max attempts exceeded for user {UserId}", user.Id);
            resetCode.IsUsed = true;
            _resetCodeRepository.Update(resetCode);
            await _resetCodeRepository.SaveChangesAsync(cancellationToken);
            throw new AuthenticationException("Invalid or expired reset code.");
        }

        var providedCodeHash = HashCode(code);
        if (providedCodeHash != resetCode.CodeHash)
        {
            _logger.LogWarning("Password reset failed: invalid code for user {UserId}, attempt {Attempt}", user.Id, resetCode.Attempts + 1);
            resetCode.Attempts++;
            _resetCodeRepository.Update(resetCode);
            await _resetCodeRepository.SaveChangesAsync(cancellationToken);
            throw new AuthenticationException("Invalid or expired reset code.");
        }

        var identityToken = await _userManager.GeneratePasswordResetTokenAsync(user);
        var result = await _userManager.ResetPasswordAsync(user, identityToken, newPassword);

        if (!result.Succeeded)
        {
            var errors = string.Join("; ", result.Errors.Select(e => e.Description));
            _logger.LogWarning("Password reset failed for user {UserId}: {Errors}", user.Id, errors);
            throw new InvalidOperationException($"Failed to reset password: {errors}");
        }

        resetCode.IsUsed = true;
        resetCode.UpdatedAt = DateTime.UtcNow;
        _resetCodeRepository.Update(resetCode);
        await _resetCodeRepository.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Password successfully reset for user {UserId}", user.Id);
        return true;
    }

    private static string HashCode(string code)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(code));
        return Convert.ToBase64String(bytes);
    }
}
