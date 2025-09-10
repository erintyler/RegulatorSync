namespace Regulator.Services.Shared.Configuration.Models;

public class TokenSettings
{
    public required int AccessTokenExpirationMinutes { get; set; }
    public required int RefreshTokenExpirationDays { get; set; }
    public required string Secret { get; set; }
    public required string Issuer { get; set; }
    public required string Audience { get; set; }
}