namespace Regulator.Services.Auth.Models;

public record Token(string TokenValue, DateTime Expiry);