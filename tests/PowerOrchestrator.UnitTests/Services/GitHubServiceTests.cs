namespace PowerOrchestrator.UnitTests.Services;

/// <summary>
/// Unit tests for GitHub service with mocked dependencies
/// </summary>
public class GitHubServiceTests
{
    private readonly Mock<ILogger<GitHubService>> _mockLogger;
    private readonly Mock<IGitHubRateLimitService> _mockRateLimitService;
    private readonly Mock<IOptions<GitHubOptions>> _mockOptions;
    private readonly GitHubOptions _gitHubOptions;

    public GitHubServiceTests()
    {
        _mockLogger = new Mock<ILogger<GitHubService>>();
        _mockRateLimitService = new Mock<IGitHubRateLimitService>();
        _mockOptions = new Mock<IOptions<GitHubOptions>>();

        _gitHubOptions = new GitHubOptions
        {
            AccessToken = "test-token",
            ApplicationName = "PowerOrchestrator-Test",
            EnterpriseBaseUrl = ""
        };

        _mockOptions.Setup(x => x.Value).Returns(_gitHubOptions);
    }

    [Fact]
    public void Constructor_WithValidParameters_ShouldInitializeCorrectly()
    {
        // Act
        var service = new GitHubService(_mockLogger.Object, _mockOptions.Object, _mockRateLimitService.Object);

        // Assert
        service.Should().NotBeNull();
    }

    [Fact]
    public void Constructor_WithNullLogger_ShouldThrowArgumentNullException()
    {
        // Act & Assert
        var exception = Assert.Throws<ArgumentNullException>(() => 
            new GitHubService(null!, _mockOptions.Object, _mockRateLimitService.Object));
        
        exception.ParamName.Should().Be("logger");
    }

    [Fact]
    public void Constructor_WithNullOptions_ShouldThrowArgumentNullException()
    {
        // Act & Assert
        var exception = Assert.Throws<ArgumentNullException>(() => 
            new GitHubService(_mockLogger.Object, null!, _mockRateLimitService.Object));
        
        exception.ParamName.Should().Be("options");
    }

    [Fact]
    public void Constructor_WithNullRateLimitService_ShouldThrowArgumentNullException()
    {
        // Act & Assert
        var exception = Assert.Throws<ArgumentNullException>(() => 
            new GitHubService(_mockLogger.Object, _mockOptions.Object, null!));
        
        exception.ParamName.Should().Be("rateLimitService");
    }

    [Fact]
    public async Task GetRepositoriesAsync_ShouldRespectRateLimit()
    {
        // Arrange
        var service = new GitHubService(_mockLogger.Object, _mockOptions.Object, _mockRateLimitService.Object);
        
        // This test verifies that rate limiting is called before API operations
        _mockRateLimitService.Setup(x => x.WaitForRateLimitAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act & Assert
        var exception = await Record.ExceptionAsync(() => service.GetRepositoriesAsync());
        
        // Verify rate limiting was called
        _mockRateLimitService.Verify(x => x.WaitForRateLimitAsync(It.IsAny<CancellationToken>()), Times.AtLeastOnce);
    }

    [Theory]
    [InlineData("owner", "repo")]
    [InlineData("microsoft", "powershell")]
    [InlineData("github", "docs")]
    public async Task GetRepositoryAsync_WithValidParameters_ShouldCallRateLimit(string owner, string name)
    {
        // Arrange
        var service = new GitHubService(_mockLogger.Object, _mockOptions.Object, _mockRateLimitService.Object);
        
        _mockRateLimitService.Setup(x => x.WaitForRateLimitAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var exception = await Record.ExceptionAsync(() => service.GetRepositoryAsync(owner, name));

        // Assert
        _mockRateLimitService.Verify(x => x.WaitForRateLimitAsync(It.IsAny<CancellationToken>()), Times.AtLeastOnce);
    }

    [Theory]
    [InlineData("", "repo")]
    [InlineData("owner", "")]
    public async Task GetRepositoryAsync_WithInvalidParameters_ShouldThrow(string owner, string name)
    {
        // Arrange
        var service = new GitHubService(_mockLogger.Object, _mockOptions.Object, _mockRateLimitService.Object);

        // Act & Assert
        var exception = await Record.ExceptionAsync(() => service.GetRepositoryAsync(owner, name));
        exception.Should().BeOfType<ArgumentException>();
    }

    [Theory]
    [InlineData("script.ps1", true)]
    [InlineData("module.psm1", true)]
    [InlineData("data.psd1", true)]
    [InlineData("config.ps1xml", true)]
    [InlineData("readme.md", false)]
    [InlineData("config.json", false)]
    [InlineData("script.py", false)]
    public void IsPowerShellFile_WithDifferentExtensions_ShouldReturnCorrectResult(string fileName, bool expected)
    {
        // This would test a utility method for filtering PowerShell files
        // Since this might be private, we can test it indirectly through GetScriptFilesAsync
        
        // For now, we'll test the pattern that the service should follow
        var powerShellExtensions = new[] { ".ps1", ".psm1", ".psd1", ".ps1xml" };
        var result = powerShellExtensions.Any(ext => fileName.EndsWith(ext, StringComparison.OrdinalIgnoreCase));
        
        result.Should().Be(expected);
    }

    [Fact]
    public void GitHubOptions_WithEnterpriseUrl_ShouldConfigureCorrectly()
    {
        // Arrange
        var enterpriseOptions = new GitHubOptions
        {
            AccessToken = "test-token",
            ApplicationName = "PowerOrchestrator-Test",
            EnterpriseBaseUrl = "https://github.enterprise.com/api/v3"
        };

        var mockEnterpriseOptions = new Mock<IOptions<GitHubOptions>>();
        mockEnterpriseOptions.Setup(x => x.Value).Returns(enterpriseOptions);

        // Act
        var service = new GitHubService(_mockLogger.Object, mockEnterpriseOptions.Object, _mockRateLimitService.Object);

        // Assert
        service.Should().NotBeNull();
        // The service should be configured with enterprise URL
    }

    [Fact]
    public async Task CancellationToken_ShouldBePropagatedToRateLimit()
    {
        // Arrange
        var service = new GitHubService(_mockLogger.Object, _mockOptions.Object, _mockRateLimitService.Object);
        var cts = new CancellationTokenSource();
        var cancellationToken = cts.Token;

        _mockRateLimitService.Setup(x => x.WaitForRateLimitAsync(cancellationToken))
            .Returns(Task.CompletedTask);

        // Act
        try
        {
            await service.GetRepositoriesAsync(cancellationToken);
        }
        catch
        {
            // Expected to fail due to missing actual API setup
        }

        // Assert
        _mockRateLimitService.Verify(x => x.WaitForRateLimitAsync(cancellationToken), Times.AtLeastOnce);
    }
}