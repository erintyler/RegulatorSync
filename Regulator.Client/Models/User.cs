using System.Diagnostics.CodeAnalysis;

namespace Regulator.Client.Models;

public class User
{
    public required string SyncCode { get; set; }
    
    [MemberNotNullWhen(true, nameof(IsOnline))]
    public ulong? CharacterId { get; set; }
    public bool IsOnline { get; set; }
}