namespace Regulator.Client.Services.Providers.Interfaces;

public interface ICharacterHashProvider
{
    ulong GetHashBySyncCode(string syncCode);
    void AddOrUpdateHash(string syncCode, ulong hash);
    void RemoveHash(string syncCode);
}