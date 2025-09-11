using Regulator.Client.Models;

namespace Regulator.Client.Services.Providers.Interfaces;

public interface IPlayerProvider
{
    Player? GetCachedPlayerBySyncCode(string syncCode);
    Player? GetCachedPlayerByHash(ulong hash);
    Player? GetPlayerByHash(string syncCode, ulong hash);
    Player? GetPendingPlayerBySyncCode(string syncCode);
}