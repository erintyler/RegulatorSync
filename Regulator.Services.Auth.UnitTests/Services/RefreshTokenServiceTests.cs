using System.Security.Cryptography;
using AutoFixture;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Regulator.Data.DynamoDb.Models;
using Regulator.Data.DynamoDb.Repositories.Interfaces;
using Regulator.Services.Auth.Services;
using Regulator.Services.Shared.Configuration.Models;

namespace Regulator.Services.Auth.UnitTests.Services;

public class RefreshTokenServiceTests
{
    private readonly Fixture _fixture = new();
    private readonly Mock<IUserRepository> _mockUserRepository = new();
    private readonly Mock<IOptions<TokenSettings>> _mockTokenSettings = new();
    private readonly Mock<ILogger<RefreshTokenService>> _mockLogger = new();
    private readonly RefreshTokenService _sut;
    
    public RefreshTokenServiceTests()
    {
        _mockTokenSettings.Setup(x => x.Value).Returns(new TokenSettings
        {
            AccessTokenExpirationMinutes = _fixture.Create<int>(),
            RefreshTokenExpirationDays = _fixture.Create<int>(),
            Secret = _fixture.Create<string>(),
            Issuer = _fixture.Create<string>(),
            Audience = _fixture.Create<string>()
        });
        
        _sut = new RefreshTokenService(_mockUserRepository.Object, _mockTokenSettings.Object, _mockLogger.Object);
    }
    
    [Fact]
    public async Task GenerateRefreshTokenAsync_ShouldNotReturnToken_WhenUserNotFound() 
    {
        // Arrange
        var discordId = _fixture.Create<string>();
        _mockUserRepository.Setup(x => x.GetAsync(discordId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        // Act
        var result = await _sut.GenerateRefreshTokenAsync(discordId, TestContext.Current.CancellationToken);

        // Assert
        _mockUserRepository.Verify(x => x.GetAsync(discordId, It.IsAny<CancellationToken>()), Times.Once);
        Assert.False(result.IsSuccess);
        Assert.Null(result.Value);
        Assert.Equal(StatusCodes.Status400BadRequest, result.StatusCode);
        Assert.Equal("Unable to generate refresh token.", result.ErrorMessage);
    }

    [Fact]
    public async Task GenerateRefreshTokenAsync_ShouldReturnToken_WhenUserFound()
    {
        // Arrange
        var discordId = _fixture.Create<string>();
        var user = new User { DiscordId = discordId, SyncCode = _fixture.Create<string>()};
        _mockUserRepository.Setup(x => x.GetAsync(discordId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);
        _mockUserRepository.Setup(x => x.UpsertAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _sut.GenerateRefreshTokenAsync(discordId, TestContext.Current.CancellationToken);

        // Assert
        _mockUserRepository.Verify(x => x.GetAsync(discordId, It.IsAny<CancellationToken>()), Times.Once);
        _mockUserRepository.Verify(x => x.UpsertAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()), Times.Once);
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.Equal(StatusCodes.Status200OK, result.StatusCode);
        Assert.False(string.IsNullOrEmpty(result.Value.TokenValue));
        Assert.True(result.Value.Expiry > DateTime.UtcNow);
        
        Assert.Equal(user.RefreshToken, GetHashedRefreshToken(result.Value.TokenValue));
        Assert.Equal(user.RefreshTokenExpiry, result.Value.Expiry);
    }
    
    [Fact]
    public async Task ValidateRefreshToken_ShouldReturnFailedResult_WhenUserNotFound() 
    {
        // Arrange
        var discordId = _fixture.Create<string>();
        var refreshToken = _fixture.Create<string>();
        _mockUserRepository.Setup(x => x.GetAsync(discordId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        // Act
        var result = await _sut.ValidateRefreshToken(discordId, refreshToken, TestContext.Current.CancellationToken);

        // Assert
        _mockUserRepository.Verify(x => x.GetAsync(discordId, It.IsAny<CancellationToken>()), Times.Once);
        Assert.False(result.IsSuccess);
        Assert.Equal("User not found.", result.ErrorMessage);
        Assert.Equal(StatusCodes.Status400BadRequest, result.StatusCode);
    }

    [Fact]
    public async Task ValidateRefreshToken_ShouldReturnFailedResult_WhenRefreshTokenExpired()
    {
        // Arrange
        var discordId = _fixture.Create<string>();
        var refreshToken = _fixture.Create<string>();
        var user = new User 
        { 
            DiscordId = discordId, 
            SyncCode = _fixture.Create<string>(),
            RefreshToken = GetHashedRefreshToken(refreshToken),
            RefreshTokenExpiry = DateTime.UtcNow.AddMinutes(-5) // Expired
        };
        _mockUserRepository.Setup(x => x.GetAsync(discordId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        // Act
        var result = await _sut.ValidateRefreshToken(discordId, refreshToken, TestContext.Current.CancellationToken);

        // Assert
        _mockUserRepository.Verify(x => x.GetAsync(discordId, It.IsAny<CancellationToken>()), Times.Once);
        Assert.False(result.IsSuccess);
        Assert.Equal("Refresh token expired.", result.ErrorMessage);
        Assert.Equal(StatusCodes.Status400BadRequest, result.StatusCode);
    }

    [Fact]
    public async Task ValidateRefreshToken_ShouldReturnFailedResult_WhenRefreshTokenInvalid()
    {
        // Arrange
        var discordId = _fixture.Create<string>();
        var refreshToken = _fixture.Create<string>();
        var user = new User 
        { 
            DiscordId = discordId, 
            SyncCode = _fixture.Create<string>(),
            RefreshToken = GetHashedRefreshToken(_fixture.Create<string>()), // Different token
            RefreshTokenExpiry = DateTime.UtcNow.AddMinutes(5) // Not expired
        };
        _mockUserRepository.Setup(x => x.GetAsync(discordId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        // Act
        var result = await _sut.ValidateRefreshToken(discordId, refreshToken, TestContext.Current.CancellationToken);

        // Assert
        _mockUserRepository.Verify(x => x.GetAsync(discordId, It.IsAny<CancellationToken>()), Times.Once);
        Assert.False(result.IsSuccess);
        Assert.Equal("Invalid refresh token.", result.ErrorMessage);
        Assert.Equal(StatusCodes.Status400BadRequest, result.StatusCode);
    }
    
    [Fact]
    public async Task ValidateRefreshToken_ShouldReturnSuccessResult_WhenRefreshTokenValid() 
    {
        // Arrange
        var discordId = _fixture.Create<string>();
        var refreshToken = _fixture.Create<string>();
        var user = new User 
        { 
            DiscordId = discordId, 
            SyncCode = _fixture.Create<string>(),
            RefreshToken = GetHashedRefreshToken(refreshToken), // Same token
            RefreshTokenExpiry = DateTime.UtcNow.AddMinutes(5) // Not expired
        };
        _mockUserRepository.Setup(x => x.GetAsync(discordId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        // Act
        var result = await _sut.ValidateRefreshToken(discordId, refreshToken, TestContext.Current.CancellationToken);

        // Assert
        _mockUserRepository.Verify(x => x.GetAsync(discordId, It.IsAny<CancellationToken>()), Times.Once);
        Assert.True(result.IsSuccess);
        Assert.Null(result.ErrorMessage);
        Assert.Equal(StatusCodes.Status200OK, result.StatusCode);
    }

    private static string GetHashedRefreshToken(string refreshToken)
    {
        var hashedBytes = SHA256.HashData(System.Text.Encoding.UTF8.GetBytes(refreshToken));
        return Convert.ToBase64String(hashedBytes);
    }
}