namespace Regulator.Services.Auth.Dtos.Requests;

public record RefreshRequestDto(string DiscordId, string RefreshToken);