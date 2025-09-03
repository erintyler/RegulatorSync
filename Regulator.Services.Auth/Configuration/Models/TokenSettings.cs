namespace Regulator.Services.Auth.Configuration.Models;

public record TokenSettings(int AccessTokenExpirationMinutes, int RefreshTokenExpirationDays, string Secret, string Issuer, string Audience);