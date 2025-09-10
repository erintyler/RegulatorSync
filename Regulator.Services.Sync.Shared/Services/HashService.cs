using System.IO.Hashing;
using Regulator.Services.Sync.Shared.Services.Interfaces;

namespace Regulator.Services.Sync.Shared.Services;

public class HashService : IHashService
{
    public ulong ComputeHash(ReadOnlySpan<byte> data)
    {
        return XxHash3.HashToUInt64(data);
    }

    public ulong ComputeHash(string data)
    {
        ReadOnlySpan<byte> bytes = System.Text.Encoding.UTF8.GetBytes(data);
        return ComputeHash(bytes);
    }
}