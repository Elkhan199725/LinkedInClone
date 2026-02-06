using Application.Auth.Commands;
using Application.Auth.Dtos;
using Application.Common.Exceptions;
using Application.Common.Interfaces;
using Domain.Constants;
using Domain.Entities;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;

namespace Application.Auth.Handlers;

public sealed class RegisterCommandHandler : IRequestHandler<RegisterCommand, AuthResponse>
{
    private readonly UserManager<AppUser> _userManager;
    private readonly IUserProfileRepository _profileRepository;
    private readonly IJwtTokenService _jwtTokenService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<RegisterCommandHandler> _logger;

    public RegisterCommandHandler(
        UserManager<AppUser> userManager,
        IUserProfileRepository profileRepository,
        IJwtTokenService jwtTokenService,
        IUnitOfWork unitOfWork,
        ILogger<RegisterCommandHandler> logger)
    {
        _userManager = userManager;
        _profileRepository = profileRepository;
        _jwtTokenService = jwtTokenService;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<AuthResponse> Handle(RegisterCommand command, CancellationToken cancellationToken)
    {
        var request = command.Request;
        var email = request.Email.Trim().ToLowerInvariant();

        _logger.LogDebug("Starting registration for email: {Email}", email);

        var existingUser = await _userManager.FindByEmailAsync(email);
        if (existingUser is not null)
        {
            _logger.LogWarning("Registration failed: email {Email} already in use", email);
            throw new RegistrationException("Email already in use.");
        }

        var user = new AppUser
        {
            Id = Guid.NewGuid(),
            Email = email,
            UserName = email,
            CreatedAt = DateTime.UtcNow
        };

        return await _unitOfWork.ExecuteInTransactionAsync(async () =>
        {
            _logger.LogDebug("Creating AppUser for {Email}", email);
            var createResult = await _userManager.CreateAsync(user, request.Password);
            if (!createResult.Succeeded)
            {
                var errors = createResult.Errors.Select(e => e.Description).ToList();
                _logger.LogWarning("Failed to create user {Email}: {Errors}", email, string.Join(", ", errors));
                throw new RegistrationException(errors);
            }

            _logger.LogDebug("Creating UserProfile for user {UserId}", user.Id);
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

            _logger.LogDebug("Adding role {Role} to user {UserId}", AppRoles.User, user.Id);
            var roleResult = await _userManager.AddToRoleAsync(user, AppRoles.User);
            if (!roleResult.Succeeded)
            {
                var errors = roleResult.Errors.Select(e => e.Description).ToList();
                _logger.LogWarning("Failed to add role to user {UserId}: {Errors}", user.Id, string.Join(", ", errors));
                throw new RegistrationException(errors);
            }

            var roles = await _userManager.GetRolesAsync(user);
            var token = _jwtTokenService.GenerateToken(user.Id, user.Email!, user.UserName, roles);

            _logger.LogInformation("Successfully registered user {UserId} with email {Email}", user.Id, email);
            return new AuthResponse(user.Id, user.Email!, roles.ToArray(), token);
        }, cancellationToken);
    }
}
