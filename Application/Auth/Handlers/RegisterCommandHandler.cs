using Application.Auth.Commands;
using Application.Auth.Dtos;
using Application.Common.Exceptions;
using Application.Common.Interfaces;
using Domain.Constants;
using Domain.Entities;
using MediatR;
using Microsoft.AspNetCore.Identity;

namespace Application.Auth.Handlers;

public sealed class RegisterCommandHandler : IRequestHandler<RegisterCommand, AuthResponse>
{
    private readonly UserManager<AppUser> _userManager;
    private readonly IUserProfileRepository _profileRepository;
    private readonly IJwtTokenService _jwtTokenService;

    public RegisterCommandHandler(
        UserManager<AppUser> userManager,
        IUserProfileRepository profileRepository,
        IJwtTokenService jwtTokenService)
    {
        _userManager = userManager;
        _profileRepository = profileRepository;
        _jwtTokenService = jwtTokenService;
    }

    public async Task<AuthResponse> Handle(RegisterCommand command, CancellationToken cancellationToken)
    {
        var request = command.Request;
        var email = request.Email.Trim().ToLowerInvariant();

        // Check if email already exists
        var existingUser = await _userManager.FindByEmailAsync(email);
        if (existingUser is not null)
            throw new RegistrationException("Email already in use.");

        // Create identity user (no profile fields)
        var user = new AppUser
        {
            Id = Guid.NewGuid(),
            Email = email,
            UserName = email,
            CreatedAt = DateTime.UtcNow
        };

        var createResult = await _userManager.CreateAsync(user, request.Password);
        if (!createResult.Succeeded)
            throw new RegistrationException(createResult.Errors.Select(e => e.Description));

        // Create UserProfile with shared PK (AppUserId = user.Id)
        var profile = new UserProfile
        {
            AppUserId = user.Id,
            FirstName = request.FirstName.Trim(),
            LastName = request.LastName.Trim(),
            IsPublic = true,
            CreatedAt = DateTime.UtcNow
        };

        await _profileRepository.AddAsync(profile, cancellationToken);
        await _profileRepository.SaveChangesAsync(cancellationToken);

        // Assign default role
        var roleResult = await _userManager.AddToRoleAsync(user, AppRoles.User);
        if (!roleResult.Succeeded)
            throw new RegistrationException(roleResult.Errors.Select(e => e.Description));

        // Get all roles and generate token
        var roles = await _userManager.GetRolesAsync(user);
        var token = _jwtTokenService.GenerateToken(user.Id, user.Email!, user.UserName, roles);

        return new AuthResponse(user.Id, user.Email!, roles.ToArray(), token);
    }
}
