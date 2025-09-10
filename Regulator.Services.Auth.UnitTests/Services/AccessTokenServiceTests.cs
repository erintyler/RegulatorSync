using AutoFixture;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Regulator.Data.DynamoDb.Models;
using Regulator.Services.Auth.Models;
using Regulator.Services.Auth.Services;
using Regulator.Services.Shared.Configuration.Models;
using Regulator.Services.Shared.Constants;
using Regulator.Services.Shared.Models;
using Regulator.Services.Shared.Services.Interfaces;

namespace Regulator.Services.Auth.UnitTests.Services;

public class AccessTokenServiceTests
{
    private readonly Fixture _fixture = new();
    private readonly Mock<IUserContextService> _mockUserContextService = new();
    private readonly Mock<IOptions<TokenSettings>> _mockTokenSettings = new();
    private readonly Mock<ILogger<AccessTokenService>> _mockLogger = new();
    private readonly AccessTokenService _sut;

    public AccessTokenServiceTests()
    {
        _mockTokenSettings.Setup(x => x.Value).Returns(new TokenSettings
        {
            AccessTokenExpirationMinutes = _fixture.Create<int>(),
            RefreshTokenExpirationDays = _fixture.Create<int>(),
            Secret = _fixture.Create<string>(),
            Issuer = _fixture.Create<string>(),
            Audience = _fixture.Create<string>()
        });
        
        _sut = new AccessTokenService(_mockUserContextService.Object, _mockTokenSettings.Object, _mockLogger.Object);
    }
    
    [Fact]
    public async Task GenerateAccessTokenAsync_ShouldReturnToken() 
    {
        // Arrange
        var discordId = _fixture.Create<string>();
        _mockUserContextService.Setup(x => x.GetCurrentUserAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<User>.Success(new User { DiscordId = discordId, SyncCode = _fixture.Create<string>()}));

        // Act
        var result = await _sut.GenerateAccessTokenAsync(TODO, TestContext.Current.CancellationToken);

        // Assert
        _mockUserContextService.Verify(x => x.GetCurrentUserAsync(It.IsAny<CancellationToken>()), Times.Once);
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.False(string.IsNullOrEmpty(result.Value.TokenValue));
        Assert.Equal(StatusCodes.Status200OK, result.StatusCode);
        
        AssertTokenValid(result, discordId, _mockTokenSettings.Object.Value);
    }

    private static void AssertTokenValid(Result<Token> result, string discordId, TokenSettings tokenSettings)
    {
        var handler = new System.IdentityModel.Tokens.Jwt.JwtSecurityTokenHandler();
        var jwtToken = handler.ReadJwtToken(result.Value!.TokenValue);
        
        var discordIdClaim = jwtToken.Claims.FirstOrDefault(c => c.Type == RegulatorClaimTypes.DiscordId);
        Assert.NotNull(discordIdClaim);
        Assert.Equal(discordId, discordIdClaim.Value);
        
        Assert.True(jwtToken.ValidTo == result.Value.Expiry);
        Assert.Equal(tokenSettings.Audience, jwtToken.Audiences.First());
        Assert.Equal(tokenSettings.Issuer, jwtToken.Issuer);
    }
}