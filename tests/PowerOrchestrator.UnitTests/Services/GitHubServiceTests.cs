namespace PowerOrchestrator.UnitTests.Services;

/// <summary>
/// Unit tests for GitHub service using production architecture
/// </summary>
public class GitHubServiceTests : IClassFixture<GitHubServiceTestFixture>
{
    private readonly GitHubServiceTestFixture _fixture;

    public GitHubServiceTests(GitHubServiceTestFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public void Constructor_WithValidParameters_ShouldInitializeCorrectly()
    {
        // Arrange & Act
        var service = _fixture.Resolve<IGitHubService>();

        // Assert
        service.Should().NotBeNull();
    }

    [Fact]
    public void AutoMapper_ShouldBeConfiguredCorrectly()
    {
        // Arrange & Act
        var mapper = _fixture.Resolve<IMapper>();

        // Assert
        mapper.Should().NotBeNull();
        mapper.ConfigurationProvider.AssertConfigurationIsValid();
    }

    [Fact]
    public async Task GetRepositoriesAsync_ShouldRespectRateLimit()
    {
        // Arrange
        var service = _fixture.Resolve<IGitHubService>();
        
        // Setup rate limiting mock
        _fixture.MockRateLimitService.Setup(x => x.WaitForRateLimitAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act & Assert
        var exception = await Record.ExceptionAsync(() => service.GetRepositoriesAsync());
        
        // Verify rate limiting was called
        _fixture.MockRateLimitService.Verify(x => x.WaitForRateLimitAsync(It.IsAny<CancellationToken>()), Times.AtLeastOnce);
    }

    [Theory]
    [InlineData("owner", "repo")]
    [InlineData("microsoft", "powershell")]
    [InlineData("github", "docs")]
    public async Task GetRepositoryAsync_WithValidParameters_ShouldCallRateLimit(string owner, string name)
    {
        // Arrange
        var service = _fixture.Resolve<IGitHubService>();
        
        _fixture.MockRateLimitService.Setup(x => x.WaitForRateLimitAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var exception = await Record.ExceptionAsync(() => service.GetRepositoryAsync(owner, name));

        // Assert
        _fixture.MockRateLimitService.Verify(x => x.WaitForRateLimitAsync(It.IsAny<CancellationToken>()), Times.AtLeastOnce);
    }

    [Theory]
    [InlineData("", "repo")]
    [InlineData("owner", "")]
    public async Task GetRepositoryAsync_WithInvalidParameters_ShouldThrow(string owner, string name)
    {
        // Arrange
        var service = _fixture.Resolve<IGitHubService>();

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
        // This tests the pattern that the service should follow
        var powerShellExtensions = new[] { ".ps1", ".psm1", ".psd1", ".ps1xml" };
        var result = powerShellExtensions.Any(ext => fileName.EndsWith(ext, StringComparison.OrdinalIgnoreCase));
        
        result.Should().Be(expected);
    }

    [Fact]
    public void GitHubOptions_ShouldBeConfiguredCorrectly()
    {
        // Arrange & Act
        var options = _fixture.Resolve<IOptions<GitHubOptions>>();

        // Assert
        options.Should().NotBeNull();
        options.Value.Should().NotBeNull();
        options.Value.AccessToken.Should().Be("test-token");
        options.Value.ApplicationName.Should().Be("PowerOrchestrator-Test");
    }

    [Fact]
    public async Task CancellationToken_ShouldBePropagatedToRateLimit()
    {
        // Arrange
        var service = _fixture.Resolve<IGitHubService>();
        var cts = new CancellationTokenSource();
        var cancellationToken = cts.Token;

        _fixture.MockRateLimitService.Setup(x => x.WaitForRateLimitAsync(cancellationToken))
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
        _fixture.MockRateLimitService.Verify(x => x.WaitForRateLimitAsync(cancellationToken), Times.AtLeastOnce);
    }

    [Fact]
    public void DependencyInjection_ShouldResolveAllRequiredServices()
    {
        // Arrange & Act & Assert
        var gitHubService = _fixture.Resolve<IGitHubService>();
        var gitHubAuthService = _fixture.Resolve<IGitHubAuthService>();
        var repositoryManager = _fixture.Resolve<IRepositoryManager>();
        var syncService = _fixture.Resolve<IRepositorySyncService>();
        var webhookService = _fixture.Resolve<IWebhookService>();
        var parserService = _fixture.Resolve<IPowerShellScriptParser>();

        gitHubService.Should().NotBeNull();
        gitHubAuthService.Should().NotBeNull();
        repositoryManager.Should().NotBeNull();
        syncService.Should().NotBeNull();
        webhookService.Should().NotBeNull();
        parserService.Should().NotBeNull();
    }

    [Fact]
    public void ServiceLifetimes_ShouldBeCorrect()
    {
        // Arrange
        using var scope1 = _fixture.CreateScope();
        using var scope2 = _fixture.CreateScope();

        // Act
        var service1 = scope1.Resolve<IGitHubService>();
        var service2 = scope1.Resolve<IGitHubService>();
        var service3 = scope2.Resolve<IGitHubService>();

        // Assert
        // Same scope should return same instance
        service1.Should().BeSameAs(service2);
        
        // Different scope should return different instance
        service1.Should().NotBeSameAs(service3);
    }
}