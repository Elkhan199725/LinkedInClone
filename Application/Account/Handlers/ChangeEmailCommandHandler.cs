using Application.Account.Commands;
using Application.Account.Dtos;
using Application.Common.Exceptions;
using Domain.Entities;
using MediatR;
using Microsoft.AspNetCore.Identity;

namespace Application.Account.Handlers;

public sealed class ChangeEmailCommandHandler : IRequestHandler<ChangeEmailCommand, ChangeEmailResponse>
{
    private readonly UserManager<AppUser> _userManager;

    public ChangeEmailCommandHandler(UserManager<AppUser> userManager)
    {
        _userManager = userManager;
    }

    public async Task<ChangeEmailResponse> Handle(ChangeEmailCommand command, CancellationToken cancellationToken)
    {
        var newEmail = command.Request.NewEmail.Trim().ToLowerInvariant();

        var user = await _userManager.FindByIdAsync(command.UserId.ToString());
        if (user is null)
            throw new NotFoundException("User", command.UserId);

        if (user.Email?.ToLowerInvariant() == newEmail)
            throw new ForbiddenException("New email must be different from current email.");

        var existingUser = await _userManager.FindByEmailAsync(newEmail);
        if (existingUser is not null && existingUser.Id != command.UserId)
            throw new ForbiddenException("This email is already in use by another account.");

        user.Email = newEmail;
        user.NormalizedEmail = newEmail.ToUpperInvariant();
        user.UserName = newEmail;
        user.NormalizedUserName = newEmail.ToUpperInvariant();
        user.UpdatedAt = DateTime.UtcNow;

        var result = await _userManager.UpdateAsync(user);
        if (!result.Succeeded)
        {
            var errors = string.Join("; ", result.Errors.Select(e => e.Description));
            throw new InvalidOperationException($"Failed to update email: {errors}");
        }

        return new ChangeEmailResponse(user.Id, user.Email);
    }
}
