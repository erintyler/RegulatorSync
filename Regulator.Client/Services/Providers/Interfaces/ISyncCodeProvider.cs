namespace Regulator.Client.Services.Providers.Interfaces;

public interface ISyncCodeProvider
{
    string GetSyncCodeByHash(ulong hash);
    void AddSyncCode(ulong hash, string syncCode);
    void RemoveSyncCode(ulong hash);
}