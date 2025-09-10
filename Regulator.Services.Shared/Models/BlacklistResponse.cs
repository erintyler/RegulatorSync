namespace Regulator.Services.Shared.Models;

public record BlacklistResponse(bool IsBlacklisted, string? Reason = null, DateTime? ExpiresAt = null);