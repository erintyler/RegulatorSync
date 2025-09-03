using Microsoft.Extensions.Logging;
using Regulator.Data.DynamoDb.Enums;
using Regulator.Data.DynamoDb.Models;
using Regulator.Data.DynamoDb.Repositories.Interfaces;
using Regulator.Services.Shared.Models;
using Regulator.Services.Shared.Services.Interfaces;

namespace Regulator.Services.Shared.Services;

public class UserCreationService(IUserRepository userRepository, ILogger<UserCreationService> logger) : IUserCreationService
{
    public async Task<Result<User>> CreateUserAsync(string discordId, CancellationToken cancellationToken = default)
    {
        var existingUser = await userRepository.GetAsync(discordId, cancellationToken);
        if (existingUser is not null)
        {
            logger.LogWarning("Attempted to create a user that already exists. DiscordId: {DiscordId}", discordId);
            return Result<User>.Failure("User already exists.");
        }

        var newUser = new User
        {
            DiscordId = discordId,
            SyncCode = Guid.NewGuid().ToString("N"),
            Role = Role.User
        };

        await userRepository.UpsertAsync(newUser, cancellationToken);
        
        logger.LogInformation("Created new user with DiscordId: {DiscordId} and SyncCode: {SyncCode}", discordId, newUser.SyncCode);
        return Result<User>.Success(newUser);
    }
}