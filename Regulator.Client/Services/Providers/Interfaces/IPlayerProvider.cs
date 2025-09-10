using Regulator.Client.Models;

namespace Regulator.Client.Services.Providers.Interfaces;

public interface IPlayerProvider
{
    Player? GetPlayerBySyncCode(string syncCode);
}