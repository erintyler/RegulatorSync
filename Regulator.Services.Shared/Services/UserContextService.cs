using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Regulator.Data.DynamoDb.Models;
using Regulator.Data.DynamoDb.Repositories.Interfaces;
using Regulator.Services.Shared.Extensions;
using Regulator.Services.Shared.Models;
using Regulator.Services.Shared.Services.Interfaces;

namespace Regulator.Services.Shared.Services;

public class UserContextService(IHttpContextAccessor contextAccessor, IUserCreationService userCreationService, IUserRepository userRepository, ILogger<UserContextService> logger) : IUserContextService
{
    private User? _currentUser;
    
    public async Task<Result<User>> GetCurrentUserAsync(CancellationToken cancellationToken = default)
    {
        var user = contextAccessor.HttpContext.User;
        
        if (user?.Identity is null || !user.Identity.IsAuthenticated)
        {
            logger.LogCritical("{MethodName} called in an unauthenticated request context", nameof(GetCurrentUserAsync));
            return Result<User>.Failure("User is not authenticated.", StatusCodes.Status401Unauthorized);
        }
        
        if (_currentUser is not null)
        {
            return Result<User>.Success(_currentUser);
        }

        var discordId = user.GetDiscordId();
        _currentUser = await userRepository.GetAsync(discordId, cancellationToken);

        if (_currentUser is not null)
        {
            return Result<User>.Success(_currentUser);
        }
        
        logger.LogInformation("User is authenticated but not found in database. DiscordId: {DiscordId}. User will be added to the database", discordId);
            
        var creationResult = await userCreationService.CreateUserAsync(discordId, cancellationToken);
        if (!creationResult.IsSuccess)
        {
            logger.LogError("Failed to create user in database for DiscordId: {DiscordId}. Error: {Error}", discordId, creationResult.ErrorMessage);
            return Result<User>.Failure("Unable to create user.", creationResult.StatusCode);
        }
            
        _currentUser = creationResult.Value;

        return Result<User>.Success(_currentUser);
    }
}