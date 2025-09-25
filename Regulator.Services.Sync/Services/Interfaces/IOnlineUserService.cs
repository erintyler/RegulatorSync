using Regulator.Data.DynamoDb.Models;
using Regulator.Services.Sync.Shared.Dtos.Client.Connections;

namespace Regulator.Services.Sync.Services.Interfaces;

public interface IOnlineUserService
{
    Task<List<OnlineUserDto>> GetOnlineSyncedUsersAsync();
    List<OnlineUserDto> GetOnlineSyncedUsers(User targetUser);
    Task SetUserOnlineAsync();
    Task UpdateCustomizationsAsync(User user, string? customizations);
    Task SetUserOfflineAsync();
}