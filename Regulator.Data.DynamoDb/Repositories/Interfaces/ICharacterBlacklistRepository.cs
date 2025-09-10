using Regulator.Data.DynamoDb.Models;

namespace Regulator.Data.DynamoDb.Repositories.Interfaces;

public interface ICharacterBlacklistRepository : IRangeKeyRepository<ulong, Guid, CharacterBlacklist>
{
}