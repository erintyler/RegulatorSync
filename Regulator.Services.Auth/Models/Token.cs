namespace Regulator.Services.Auth.Models;

public record RefreshToken(string Token, DateTime Expiry);