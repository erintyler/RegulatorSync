using System;
using Regulator.Services.Sync.Shared.Services;

namespace Regulator.Client.Models;

public record Player(uint ObjectId, string Name, int WorldId, IntPtr Address, ushort ObjectIndex, string SyncCode)
{
}