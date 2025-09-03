using AutoFixture;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Moq;
using Regulator.Data.DynamoDb.Models;
using Regulator.Data.DynamoDb.Repositories.Interfaces;
using Regulator.Services.Shared.Models;
using Regulator.Services.Shared.Services;
using Regulator.Services.Shared.Services.Interfaces;
using Regulator.Tests.Utilities.Helpers;

namespace Regulator.Services.Shared.UnitTests.Services;

public class UserContextServiceTests
{
    private readonly Fixture _fixture = new();
    private readonly Mock<IHttpContextAccessor> _mockHttpContextAccessor = new();
    private readonly Mock<IUserCreationService> _mockUserCreationService = new();
    private readonly Mock<IUserRepository> _mockUserRepository = new();
    private readonly Mock<ILogger<UserContextService>> _mockLogger = new();
    private readonly UserContextService _sut;
    
    public UserContextServiceTests()
    {
        _sut = new UserContextService(_mockHttpContextAccessor.Object, _mockUserCreationService.Object, _mockUserRepository.Object, _mockLogger.Object);
    }

    [Fact]
    public async Task GetCurrentUserAsync_ShouldReturnFailure_WhenUserIsNotAuthenticated()
    {
        // Arrange
        var mockHttpContext = new DefaultHttpContext();
        
        _mockHttpContextAccessor.Setup(x => x.HttpContext).Returns(mockHttpContext);
        
        // Act
        var result = await _sut.GetCurrentUserAsync(TestContext.Current.CancellationToken);
        
        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(StatusCodes.Status401Unauthorized, result.StatusCode);
        Assert.Equal("User is not authenticated.", result.ErrorMessage);
    }
    
    [Fact]
    public async Task GetCurrentUserAsync_ShouldReturnExistingUser_WhenUserIsFoundInDatabase()
    {
        // Arrange
        var discordId = _fixture.Create<string>();
        var existingUser = new User
        {
            DiscordId = discordId,
            SyncCode = _fixture.Create<string>()
        };
        
        var claimsPrincipal = ClaimsHelper.GetClaimsPrincipal(discordId);
        var mockHttpContext = new DefaultHttpContext
        {
            User = claimsPrincipal
        };
        
        _mockHttpContextAccessor.Setup(x => x.HttpContext).Returns(mockHttpContext);
        _mockUserRepository.Setup(x => x.GetAsync(discordId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingUser);
        
        // Act
        var result = await _sut.GetCurrentUserAsync(TestContext.Current.CancellationToken);
        
        // Assert
        _mockUserRepository.Verify(x => x.GetAsync(discordId, It.IsAny<CancellationToken>()), Times.Once);
        Assert.True(result.IsSuccess);
        Assert.Equal(existingUser, result.Value);
    }

    [Fact]
    public async Task GetCurrentUserAsync_ShouldCreateAndReturnNewUser_WhenUserIsNotFoundInDatabase()
    {
        // Arrange
        var discordId = _fixture.Create<string>();
        var newUser = new User
        {
            DiscordId = discordId,
            SyncCode = _fixture.Create<string>()
        };
        
        var claimsPrincipal = ClaimsHelper.GetClaimsPrincipal(discordId);
        var mockHttpContext = new DefaultHttpContext
        {
            User = claimsPrincipal
        };
        
        _mockHttpContextAccessor.Setup(x => x.HttpContext).Returns(mockHttpContext);
        _mockUserRepository.Setup(x => x.GetAsync(discordId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);
        _mockUserCreationService.Setup(x => x.CreateUserAsync(discordId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<User>.Success(newUser));
        
        // Act
        var result = await _sut.GetCurrentUserAsync(TestContext.Current.CancellationToken);
        
        // Assert
        _mockUserRepository.Verify(x => x.GetAsync(discordId, It.IsAny<CancellationToken>()), Times.Once);
        _mockUserCreationService.Verify(x => x.CreateUserAsync(discordId, It.IsAny<CancellationToken>()), Times.Once);
        Assert.True(result.IsSuccess);
        Assert.Equal(newUser, result.Value);
    }
    
    [Fact]
    public async Task GetCurrentUserAsync_ShouldReturnFailure_WhenUserCreationFails()
    {
        // Arrange
        var discordId = _fixture.Create<string>();
        
        var claimsPrincipal = ClaimsHelper.GetClaimsPrincipal(discordId);
        var mockHttpContext = new DefaultHttpContext
        {
            User = claimsPrincipal
        };
        
        _mockHttpContextAccessor.Setup(x => x.HttpContext).Returns(mockHttpContext);
        _mockUserRepository.Setup(x => x.GetAsync(discordId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);
        _mockUserCreationService.Setup(x => x.CreateUserAsync(discordId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<User>.Failure("Creation failed"));
        
        // Act
        var result = await _sut.GetCurrentUserAsync(TestContext.Current.CancellationToken);
        
        // Assert
        _mockUserRepository.Verify(x => x.GetAsync(discordId, It.IsAny<CancellationToken>()), Times.Once);
        _mockUserCreationService.Verify(x => x.CreateUserAsync(discordId, It.IsAny<CancellationToken>()), Times.Once);
        Assert.False(result.IsSuccess);
        Assert.Equal(StatusCodes.Status500InternalServerError, result.StatusCode);
        Assert.Equal("Unable to create user.", result.ErrorMessage);
    }
    
    [Fact]
    public async Task GetCurrentUserAsync_ShouldReturnCachedUser_OnSubsequentCalls()
    {
        // Arrange
        var discordId = _fixture.Create<string>();
        var existingUser = new User
        {
            DiscordId = discordId,
            SyncCode = _fixture.Create<string>()
        };
        
        var claimsPrincipal = ClaimsHelper.GetClaimsPrincipal(discordId);
        var mockHttpContext = new DefaultHttpContext
        {
            User = claimsPrincipal
        };
        
        _mockHttpContextAccessor.Setup(x => x.HttpContext).Returns(mockHttpContext);
        _mockUserRepository.Setup(x => x.GetAsync(discordId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingUser);
        
        // Act
        var firstCallResult = await _sut.GetCurrentUserAsync(TestContext.Current.CancellationToken);
        var secondCallResult = await _sut.GetCurrentUserAsync(TestContext.Current.CancellationToken);
        
        // Assert
        _mockUserRepository.Verify(x => x.GetAsync(discordId, It.IsAny<CancellationToken>()), Times.Once);
        Assert.True(firstCallResult.IsSuccess);
        Assert.Equal(existingUser, firstCallResult.Value);
        
        Assert.True(secondCallResult.IsSuccess);
        Assert.Equal(existingUser, secondCallResult.Value);
    }
}