namespace Regulator.Data.Redis.Constants;

public static class CacheKeys
{
    public static string SyncRequest(Guid id) => $"sr:{id}";
    public static string OnlineUser(string syncCode) => $"ou:{syncCode}";
}