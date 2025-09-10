using System.IO.Hashing;

namespace Regulator.Services.Sync.Shared.Services.Interfaces;

public interface IHashService
{
    ulong ComputeHash(ReadOnlySpan<byte> data);
    ulong ComputeHash(string data);
}