using Application.Admin.Commands;
using Application.Common.Exceptions;
using Application.Common.Interfaces;
using Domain.Constants;
using Domain.Entities;
using MediatR;
using Microsoft.AspNetCore.Identity;

namespace Application.Admin.Handlers;

public sealed class AdminDeleteUserCommandHandler : IRequestHandler<AdminDeleteUserCommand, bool>
{
    private readonly UserManager<AppUser> _userManager;
    private readonly IAccountDeletionService _accountDeletionService;

    public AdminDeleteUserCommandHandler(
        UserManager<AppUser> userManager,
        IAccountDeletionService accountDeletionService)
    {
        _userManager = userManager;
        _accountDeletionService = accountDeletionService;
    }

    public async Task<bool> Handle(AdminDeleteUserCommand command, CancellationToken cancellationToken)
    {
        if (command.TargetUserId == command.RequestingUserId)
            throw new ForbiddenException("You cannot delete your own account via admin endpoint. Use DELETE /api/profile/me instead.");

        var targetUser = await _userManager.FindByIdAsync(command.TargetUserId.ToString());

        if (targetUser is null)
            throw new NotFoundException("User", command.TargetUserId);

        var targetUserRoles = await _userManager.GetRolesAsync(targetUser);

        if (targetUserRoles.Contains(AppRoles.SuperAdmin))
            throw new ForbiddenException("Cannot delete a SuperAdmin user.");

        await _accountDeletionService.DeleteAccountAsync(command.TargetUserId, cancellationToken);

        return true;
    }
}
