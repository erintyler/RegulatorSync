using AutoFixture;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Moq;
using Regulator.Data.DynamoDb.Repositories.Interfaces;
using Regulator.Services.Shared.Services;
using Regulator.Services.Shared.Services.Interfaces;

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
}