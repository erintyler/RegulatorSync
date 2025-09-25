using Regulator.Data.DynamoDb.Models;
using Regulator.Data.Redis.Models;
using Regulator.Data.Redis.Services.Interfaces;
using Regulator.Services.Shared.Services.Interfaces;
using Regulator.Services.Sync.Services.Interfaces;
using Regulator.Services.Sync.Shared.Dtos.Client.Connections;

namespace Regulator.Services.Sync.Services;

public class OnlineUserService(
    IUserContextService userContextService, 
    IOnlineUserRepository onlineUserRepository,
    ILogger<OnlineUserService> logger) : IOnlineUserService
{
    public async Task<List<OnlineUserDto>> GetOnlineSyncedUsersAsync()
    {
        var user = await GetCurrentUserAsync();

        var onlineUsers = GetOnlineUserDtos(user.AddedSyncCodes);
        
        logger.LogInformation("User {User} has {Count} online synced users.", user.SyncCode, onlineUsers.Count);
        
        return onlineUsers;
    }
    
    public List<OnlineUserDto> GetOnlineSyncedUsers(User targetUser)
    {
        var onlineUsers = GetOnlineUserDtos(targetUser.AddedSyncCodes);
        
        logger.LogInformation("Retrieved {Count} online synced users for target user {TargetUser}.", onlineUsers.Count, targetUser.SyncCode);
        
        return onlineUsers;
    }

    public async Task SetUserOnlineAsync()
    {
        var user = await GetCurrentUserAsync();
        var characterId = GetCurrentCharacterId();
        
        var onlineUser = new OnlineUser
        {
            Id = user.SyncCode,
            CharacterId = characterId,
        };
        
        await onlineUserRepository.SaveAsync(onlineUser);
        
        logger.LogInformation("User {User} set to online with character ID {CharacterId}.", user.SyncCode, characterId);
    }

    public async Task UpdateCustomizationsAsync(User user, string? customizations)
    {
        var onlineUser = await onlineUserRepository.GetByIdAsync(user.SyncCode);
        if (onlineUser is null)
        {
            logger.LogWarning("Attempted to set customizations for offline user {User}.", user.SyncCode);
            return;
        }

        onlineUser.CurrentCustomizations = customizations;
        await onlineUserRepository.SaveAsync(onlineUser);
        
        logger.LogInformation("Set customizations for user {User}.", user.SyncCode);
    }

    public async Task SetUserOfflineAsync()
    {
        var user = await GetCurrentUserAsync();
        await onlineUserRepository.DeleteAsync(user.SyncCode);
        
        logger.LogInformation("User {User} set to offline.", user.SyncCode);
    }

    private async Task<User> GetCurrentUserAsync(CancellationToken cancellationToken = default)
    {
        var userResult = await userContextService.GetCurrentUserAsync(cancellationToken);

        if (!userResult.IsSuccess)
        {
            throw new InvalidOperationException(userResult.ErrorMessage);
        }

        return userResult.Value;
    }
    
    private List<OnlineUserDto> GetOnlineUserDtos(List<string> syncCodes)
    {
        var onlineUsers = syncCodes
            .Select(async syncCode => await onlineUserRepository.GetByIdAsync(syncCode))
            .Select(task => task.Result)
            .Where(u => u is not null)
            .Select(u => new OnlineUserDto
            {
                SyncCode = u!.Id,
                CharacterId = u.CharacterId,
                CurrentCustomizations = u.CurrentCustomizations,
            })
            .ToList();
        
        return onlineUsers;
    }
    
    private ulong GetCurrentCharacterId()
    {
        var characterIdResult = userContextService.GetCurrentCharacterId();

        if (!characterIdResult.IsSuccess)
        {
            throw new InvalidOperationException(characterIdResult.ErrorMessage);
        }

        return characterIdResult.Value;
    }
}