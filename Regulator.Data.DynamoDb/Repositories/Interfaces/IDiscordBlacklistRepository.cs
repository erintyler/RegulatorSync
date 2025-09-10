using Regulator.Data.DynamoDb.Models;

namespace Regulator.Data.DynamoDb.Repositories.Interfaces;

public interface IDiscordBlacklistRepository : IRangeKeyRepository<string, Guid, DiscordBlacklist>
{
}