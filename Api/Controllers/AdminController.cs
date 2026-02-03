using Domain.Constants;
using Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = AppRoles.SuperAdmin)]
public sealed class AdminController : ControllerBase
{
    private readonly UserManager<AppUser> _userManager;

    public AdminController(UserManager<AppUser> userManager)
    {
        _userManager = userManager;
    }

    public sealed record SetRoleRequest(Guid UserId, string Role);

    [HttpPost("set-role")]
    public async Task<IActionResult> SetRole(SetRoleRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Role))
            return BadRequest("Role is required.");

        var role = AppRoles.All
            .FirstOrDefault(r => r.Equals(request.Role, StringComparison.OrdinalIgnoreCase));

        if (role is null)
            return BadRequest("Invalid role.");

        // Prevent self-demotion
        var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (currentUserId == request.UserId.ToString() && role != AppRoles.SuperAdmin)
            return BadRequest("You cannot remove your own SuperAdmin role.");

        var user = await _userManager.FindByIdAsync(request.UserId.ToString());
        if (user is null)
            return NotFound("User not found.");

        var currentRoles = await _userManager.GetRolesAsync(user);
        if (currentRoles.Any())
        {
            var removeResult = await _userManager.RemoveFromRolesAsync(user, currentRoles);
            if (!removeResult.Succeeded)
                return StatusCode(500, removeResult.Errors.Select(e => e.Description));
        }

        var addResult = await _userManager.AddToRoleAsync(user, role);
        if (!addResult.Succeeded)
            return StatusCode(500, addResult.Errors.Select(e => e.Description));

        return Ok(new
        {
            user.Id,
            role
        });
    }
}
