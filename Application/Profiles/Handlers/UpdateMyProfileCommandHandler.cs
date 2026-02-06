using Application.Common.Exceptions;
using Application.Common.Interfaces;
using Application.Profiles.Commands;
using Application.Profiles.Dtos;
using AutoMapper;
using Domain.Entities;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;

namespace Application.Profiles.Handlers;

public sealed class UpdateMyProfileCommandHandler : IRequestHandler<UpdateMyProfileCommand, MyProfileResponse>
{
    private readonly IUserProfileRepository _repository;
    private readonly UserManager<AppUser> _userManager;
    private readonly IMapper _mapper;
    private readonly ILogger<UpdateMyProfileCommandHandler> _logger;

    public UpdateMyProfileCommandHandler(
        IUserProfileRepository repository,
        UserManager<AppUser> userManager,
        IMapper mapper,
        ILogger<UpdateMyProfileCommandHandler> logger)
    {
        _repository = repository;
        _userManager = userManager;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<MyProfileResponse> Handle(UpdateMyProfileCommand command, CancellationToken cancellationToken)
    {
        _logger.LogDebug("Updating profile for user {UserId}", command.AppUserId);

        var user = await _userManager.FindByIdAsync(command.AppUserId.ToString());
        if (user is null)
        {
            _logger.LogWarning("Update profile failed: user {UserId} not found", command.AppUserId);
            throw new NotFoundException("User", command.AppUserId);
        }

        var profile = await _repository.GetByIdAsync(command.AppUserId, cancellationToken);

        if (profile is null)
        {
            _logger.LogInformation("Creating new profile for existing user {UserId}", command.AppUserId);
            profile = new UserProfile
            {
                AppUserId = command.AppUserId,
                IsPublic = true,
                CreatedAt = DateTime.UtcNow
            };

            _mapper.Map(command.Request, profile);

            await _repository.AddAsync(profile, cancellationToken);
        }
        else
        {
            _logger.LogDebug("Updating existing profile for user {UserId}", command.AppUserId);
            _mapper.Map(command.Request, profile);
            profile.UpdatedAt = DateTime.UtcNow;

            _repository.Update(profile);
        }

        await _repository.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Successfully updated profile for user {UserId}", command.AppUserId);
        return _mapper.Map<MyProfileResponse>(profile);
    }
}
