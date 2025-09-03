namespace Regulator.Services.Auth.Dtos.Responses;

public record TokenResponseDto(string AccessToken, DateTime AccessTokenExpiry, string RefreshToken, DateTime RefreshTokenExpiry);