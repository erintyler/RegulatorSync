using AutoFixture;
using Microsoft.Extensions.Logging;
using Moq;
using Regulator.Data.DynamoDb.Repositories.Interfaces;
using Regulator.Services.Shared.Services;

namespace Regulator.Services.Shared.UnitTests.Services;

public class UserCreationServiceTests
{
    private readonly Fixture _fixture = new();
    private readonly Mock<IUserRepository> _mockUserRepository = new();
    private readonly Mock<ILogger<UserCreationService>> _mockLogger = new();
    private readonly UserCreationService _sut;
    
    public UserCreationServiceTests()
    {
        _sut = new UserCreationService(_mockUserRepository.Object, _mockLogger.Object);
    }
    
    [Fact]
    public async Task CreateUserAsync_ShouldReturnFailure_WhenUserAlreadyExists() 
    {
        // Arrange
        var discordId = _fixture.Create<string>();
        var existingUser = new Regulator.Data.DynamoDb.Models.User
        {
            DiscordId = discordId,
            SyncCode = _fixture.Create<string>()
        };
        
        _mockUserRepository.Setup(x => x.GetAsync(discordId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingUser);
        
        // Act
        var result = await _sut.CreateUserAsync(discordId, TestContext.Current.CancellationToken);
        
        // Assert
        _mockUserRepository.Verify(x => x.GetAsync(discordId, It.IsAny<CancellationToken>()), Times.Once);
        Assert.False(result.IsSuccess);
        Assert.Equal("User already exists.", result.ErrorMessage);
    }

    [Fact]
    public async Task CreateUserAsync_ShouldCreateAndReturnNewUser_WhenUserDoesNotExist()
    {
        // Arrange
        var discordId = _fixture.Create<string>();
        
        _mockUserRepository.Setup(x => x.GetAsync(discordId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Regulator.Data.DynamoDb.Models.User?)null);
        
        _mockUserRepository.Setup(x => x.UpsertAsync(It.IsAny<Regulator.Data.DynamoDb.Models.User>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        
        // Act
        var result = await _sut.CreateUserAsync(discordId, TestContext.Current.CancellationToken);
        
        // Assert
        _mockUserRepository.Verify(x => x.GetAsync(discordId, It.IsAny<CancellationToken>()), Times.Once);
        _mockUserRepository.Verify(x => x.UpsertAsync(It.IsAny<Regulator.Data.DynamoDb.Models.User>(), It.IsAny<CancellationToken>()), Times.Once);
        
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.Equal(discordId, result.Value.DiscordId);
        Assert.False(string.IsNullOrEmpty(result.Value.SyncCode));
    }
}