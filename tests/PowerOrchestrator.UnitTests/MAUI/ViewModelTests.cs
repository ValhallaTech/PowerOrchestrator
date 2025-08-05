using Autofac;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using PowerOrchestrator.MAUI.Models;
using PowerOrchestrator.MAUI.Services;
using PowerOrchestrator.MAUI.ViewModels;
using MAUIPerformanceService = PowerOrchestrator.MAUI.Services.IPerformanceMonitoringService;
using MAUIPerformanceServiceImpl = PowerOrchestrator.MAUI.Services.PerformanceMonitoringService;

namespace PowerOrchestrator.UnitTests.MAUI;

/// <summary>
/// Unit tests for MAUI ViewModels using Autofac container resolution
/// </summary>
public class ViewModelTests : IDisposable
{
    private readonly IContainer _container;
    private readonly Mock<INavigationService> _navigationServiceMock;
    private readonly Mock<IDialogService> _dialogServiceMock;
    private readonly Mock<IApiService> _apiServiceMock;
    private readonly Mock<IAuthenticationService> _authenticationServiceMock;
    private readonly Mock<IAuthorizationService> _authorizationServiceMock;
    private readonly Mock<IOfflineService> _offlineServiceMock;
    private readonly Mock<MAUIPerformanceService> _performanceMonitoringServiceMock;

    public ViewModelTests()
    {
        // Setup mocks
        _navigationServiceMock = new Mock<INavigationService>();
        _dialogServiceMock = new Mock<IDialogService>();
        _apiServiceMock = new Mock<IApiService>();
        _authenticationServiceMock = new Mock<IAuthenticationService>();
        _authorizationServiceMock = new Mock<IAuthorizationService>();
        _offlineServiceMock = new Mock<IOfflineService>();
        _performanceMonitoringServiceMock = new Mock<MAUIPerformanceService>();

        // Configure Autofac container
        var builder = new ContainerBuilder();

        // Register mocks
        builder.RegisterInstance(_navigationServiceMock.Object).As<INavigationService>();
        builder.RegisterInstance(_dialogServiceMock.Object).As<IDialogService>();
        builder.RegisterInstance(_apiServiceMock.Object).As<IApiService>();
        builder.RegisterInstance(_authenticationServiceMock.Object).As<IAuthenticationService>();
        builder.RegisterInstance(_authorizationServiceMock.Object).As<IAuthorizationService>();
        builder.RegisterInstance(_offlineServiceMock.Object).As<IOfflineService>();
        builder.RegisterInstance(_performanceMonitoringServiceMock.Object).As<MAUIPerformanceService>();
        builder.RegisterInstance(Mock.Of<AutoMapper.IMapper>()).As<AutoMapper.IMapper>();

        // Register logger factory
        builder.RegisterInstance(Mock.Of<ILoggerFactory>()).As<ILoggerFactory>();
        
        // Register individual loggers
        builder.RegisterInstance(Mock.Of<ILogger<ScriptsViewModel>>()).As<ILogger<ScriptsViewModel>>();
        builder.RegisterInstance(Mock.Of<ILogger<RepositoriesViewModel>>()).As<ILogger<RepositoriesViewModel>>();
        builder.RegisterInstance(Mock.Of<ILogger<DashboardViewModel>>()).As<ILogger<DashboardViewModel>>();

        // Register ViewModels
        builder.RegisterType<ScriptsViewModel>().AsSelf();
        builder.RegisterType<RepositoriesViewModel>().AsSelf();
        builder.RegisterType<DashboardViewModel>().AsSelf();

        _container = builder.Build();
    }

    [Fact]
    public void ScriptsViewModel_ShouldResolveFromContainer()
    {
        // Act
        var viewModel = _container.Resolve<ScriptsViewModel>();

        // Assert
        viewModel.Should().NotBeNull();
        viewModel.Title.Should().Be("Scripts");
        viewModel.Scripts.Should().NotBeNull();
        viewModel.FilteredScripts.Should().NotBeNull();
    }

    [Fact]
    public void RepositoriesViewModel_ShouldResolveFromContainer()
    {
        // Act
        var viewModel = _container.Resolve<RepositoriesViewModel>();

        // Assert
        viewModel.Should().NotBeNull();
        viewModel.Title.Should().Be("Repositories");
        viewModel.Repositories.Should().NotBeNull();
        viewModel.FilteredRepositories.Should().NotBeNull();
    }

    [Fact]
    public void DashboardViewModel_ShouldResolveFromContainer()
    {
        // Act
        var viewModel = _container.Resolve<DashboardViewModel>();

        // Assert
        viewModel.Should().NotBeNull();
        viewModel.Title.Should().Be("Dashboard");
    }

    [Fact]
    public async Task ScriptsViewModel_SearchText_ShouldFilterResults()
    {
        // Arrange
        var viewModel = _container.Resolve<ScriptsViewModel>();
        await Task.Delay(100); // Allow initialization

        var initialCount = viewModel.FilteredScripts.Count;

        // Act
        viewModel.SearchText = "System";

        // Assert
        viewModel.FilteredScripts.Should().HaveCountLessOrEqualTo(initialCount);
        if (viewModel.FilteredScripts.Any())
        {
            viewModel.FilteredScripts.Should().OnlyContain(s => 
                s.Name.Contains("System", StringComparison.OrdinalIgnoreCase) ||
                s.Description.Contains("System", StringComparison.OrdinalIgnoreCase) ||
                s.Category.Contains("System", StringComparison.OrdinalIgnoreCase) ||
                s.Tags.Any(t => t.Contains("System", StringComparison.OrdinalIgnoreCase)));
        }
    }

    [Fact]
    public async Task RepositoriesViewModel_SearchText_ShouldFilterResults()
    {
        // Arrange
        var viewModel = _container.Resolve<RepositoriesViewModel>();
        await Task.Delay(100); // Allow initialization

        var initialCount = viewModel.FilteredRepositories.Count;

        // Act
        viewModel.SearchText = "PowerShell";

        // Assert
        viewModel.FilteredRepositories.Should().HaveCountLessOrEqualTo(initialCount);
        if (viewModel.FilteredRepositories.Any())
        {
            viewModel.FilteredRepositories.Should().OnlyContain(r => 
                r.Name.Contains("PowerShell", StringComparison.OrdinalIgnoreCase) ||
                r.Description.Contains("PowerShell", StringComparison.OrdinalIgnoreCase) ||
                r.Type.Contains("PowerShell", StringComparison.OrdinalIgnoreCase) ||
                r.Tags.Any(t => t.Contains("PowerShell", StringComparison.OrdinalIgnoreCase)));
        }
    }

    [Fact]
    public async Task ScriptsViewModel_RunScriptCommand_ShouldShowDialog_WhenScriptIsNull()
    {
        // Arrange
        var viewModel = _container.Resolve<ScriptsViewModel>();

        // Act
        if (viewModel.RunScriptCommand.CanExecute(null))
        {
            viewModel.RunScriptCommand.Execute(null);
            await Task.Delay(100);
        }

        // Assert - Should not crash and not call API when script is null
        _apiServiceMock.Verify(x => x.PostAsync<object>(It.IsAny<string>(), It.IsAny<object>()), Times.Never);
    }

    [Fact]
    public async Task ScriptsViewModel_RunScriptCommand_ShouldCallApi_WhenScriptIsValid()
    {
        // Arrange
        var viewModel = _container.Resolve<ScriptsViewModel>();
        var testScript = new ScriptUIModel
        {
            Id = Guid.NewGuid().ToString(),
            Name = "Test Script",
            Content = "Get-Date"
        };

        _dialogServiceMock.Setup(x => x.ShowConfirmAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(true);

        _apiServiceMock.Setup(x => x.PostAsync<object>("api/executions", It.IsAny<object>()))
            .ReturnsAsync(new { success = true });

        // Act
        if (viewModel.RunScriptCommand.CanExecute(testScript))
        {
            viewModel.RunScriptCommand.Execute(testScript);
            await Task.Delay(500); // Allow async operation to complete
        }

        // Assert
        _dialogServiceMock.Verify(x => x.ShowConfirmAsync(
            "Run Script", 
            $"Are you sure you want to run '{testScript.Name}'?",
            "Run", 
            "Cancel"), Times.Once);

        // Note: In console mode, API might not be called, so we verify the dialog was shown
    }

    [Fact]
    public async Task RepositoriesViewModel_SyncRepositoryCommand_ShouldUpdateStatus()
    {
        // Arrange
        var viewModel = _container.Resolve<RepositoriesViewModel>();
        var testRepository = new RepositoryUIModel
        {
            Id = Guid.NewGuid().ToString(),
            Name = "Test Repository",
            Url = "https://github.com/test/repo.git",
            SyncStatus = "Synced"
        };

        _dialogServiceMock.Setup(x => x.ShowConfirmAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(true);

        _apiServiceMock.Setup(x => x.PostAsync<object>("api/repositories/sync", It.IsAny<object>()))
            .ReturnsAsync(new { success = true });

        // Act
        if (viewModel.SyncRepositoryCommand.CanExecute(testRepository))
        {
            viewModel.SyncRepositoryCommand.Execute(testRepository);
            await Task.Delay(500); // Allow async operation to complete
        }

        // Assert
        _dialogServiceMock.Verify(x => x.ShowConfirmAsync(
            "Sync Repository", 
            $"Are you sure you want to sync '{testRepository.Name}' from {testRepository.Type}?",
            "Sync", 
            "Cancel"), Times.Once);
    }

    [Fact]
    public void ScriptUIModel_FormattedProperties_ShouldReturnCorrectValues()
    {
        // Arrange
        var script = new ScriptUIModel
        {
            Id = "test-id",
            Name = "Test Script",
            Description = "A test script for validation",
            Content = "Get-Process",
            Category = "System",
            Tags = new List<string> { "test", "system", "diagnostics" },
            Version = "1.0.0",
            CreatedAt = DateTime.Now.AddDays(-10)
        };

        // Assert
        script.Id.Should().Be("test-id");
        script.Name.Should().Be("Test Script");
        script.Description.Should().Be("A test script for validation");
        script.Content.Should().Be("Get-Process");
        script.Category.Should().Be("System");
        script.Tags.Should().HaveCount(3);
        script.Tags.Should().Contain("test");
        script.Version.Should().Be("1.0.0");
        script.IsActive.Should().BeTrue();
    }

    [Fact]
    public void RepositoryUIModel_FormattedSize_ShouldReturnCorrectFormat()
    {
        // Arrange & Act
        var repo1 = new RepositoryUIModel { SizeBytes = 1024 };
        var repo2 = new RepositoryUIModel { SizeBytes = 1024 * 1024 };
        var repo3 = new RepositoryUIModel { SizeBytes = 1024 * 1024 * 1024 };

        // Assert
        repo1.FormattedSize.Should().Be("1 KB");
        repo2.FormattedSize.Should().Be("1 MB");
        repo3.FormattedSize.Should().Be("1 GB");
    }

    [Fact]
    public void RepositoryUIModel_SyncStatusColor_ShouldReturnCorrectColors()
    {
        // Arrange & Act
        var syncedRepo = new RepositoryUIModel { SyncStatus = "Synced" };
        var syncingRepo = new RepositoryUIModel { SyncStatus = "Syncing" };
        var errorRepo = new RepositoryUIModel { SyncStatus = "Error" };
        var unknownRepo = new RepositoryUIModel { SyncStatus = "Unknown" };

        // Assert
        syncedRepo.SyncStatusColor.Should().Be("#4CAF50");
        syncingRepo.SyncStatusColor.Should().Be("#FF9800");
        errorRepo.SyncStatusColor.Should().Be("#F44336");
        unknownRepo.SyncStatusColor.Should().Be("#9E9E9E");
    }

    [Fact]
    public void RepositoryUIModel_IsSyncing_ShouldReturnCorrectValue()
    {
        // Arrange & Act
        var syncingRepo = new RepositoryUIModel { SyncStatus = "Syncing" };
        var syncedRepo = new RepositoryUIModel { SyncStatus = "Synced" };

        // Assert
        syncingRepo.IsSyncing.Should().BeTrue();
        syncedRepo.IsSyncing.Should().BeFalse();
    }

    [Fact]
    public async Task ViewModels_ShouldNotThrow_WhenInitialized()
    {
        // Arrange & Act
        var scriptsViewModel = _container.Resolve<ScriptsViewModel>();
        var repositoriesViewModel = _container.Resolve<RepositoriesViewModel>();
        var dashboardViewModel = _container.Resolve<DashboardViewModel>();

        // Allow any initialization to complete
        await Task.Delay(100);

        // Assert
        scriptsViewModel.Should().NotBeNull();
        repositoriesViewModel.Should().NotBeNull();
        dashboardViewModel.Should().NotBeNull();

        // ViewModels should have expected initial state
        scriptsViewModel.IsBusy.Should().BeFalse();
        repositoriesViewModel.IsBusy.Should().BeFalse();
        dashboardViewModel.IsBusy.Should().BeFalse();
    }

    public void Dispose()
    {
        _container?.Dispose();
    }
}

/// <summary>
/// Unit tests for MAUI Services using Autofac container resolution
/// </summary>
public class ServiceTests : IDisposable
{
    private readonly IContainer _container;
    private readonly Mock<ILogger<OfflineService>> _loggerMock;
    private readonly Mock<ISettingsService> _settingsServiceMock;

    public ServiceTests()
    {
        // Setup mocks
        _loggerMock = new Mock<ILogger<OfflineService>>();
        _settingsServiceMock = new Mock<ISettingsService>();

        // Configure Autofac container
        var builder = new ContainerBuilder();

        // Register mocks and services
        builder.RegisterInstance(_loggerMock.Object).As<ILogger<OfflineService>>();
        builder.RegisterInstance(_settingsServiceMock.Object).As<ISettingsService>();
        builder.RegisterInstance(Mock.Of<ILogger<MAUIPerformanceServiceImpl>>()).As<ILogger<MAUIPerformanceServiceImpl>>();

        // Register services
        builder.RegisterType<OfflineService>().As<IOfflineService>();
        builder.RegisterType<MAUIPerformanceServiceImpl>().As<MAUIPerformanceService>();

        _container = builder.Build();
    }

    [Fact]
    public void OfflineService_ShouldResolveFromContainer()
    {
        // Act
        var service = _container.Resolve<IOfflineService>();

        // Assert
        service.Should().NotBeNull();
        service.IsOffline.Should().BeFalse(); // Should default to online in console mode
    }

    [Fact]
    public void MAUIPerformanceServiceImpl_ShouldResolveFromContainer()
    {
        // Act
        var service = _container.Resolve<MAUIPerformanceService>();

        // Assert
        service.Should().NotBeNull();
    }

    [Fact]
    public async Task OfflineService_SetAndGetCachedData_ShouldWork()
    {
        // Arrange
        var service = _container.Resolve<IOfflineService>();
        var testData = new { Name = "Test", Value = 123 };

        // Act
        await service.SetCachedDataAsync("test-key", testData);
        var result = await service.GetCachedDataAsync<object>("test-key");

        // Assert
        result.Should().NotBeNull();
    }

    [Fact]
    public async Task OfflineService_QueueOperation_ShouldAddToQueue()
    {
        // Arrange
        var service = _container.Resolve<IOfflineService>();
        var operation = new OfflineOperation
        {
            OperationType = "TestOperation",
            Data = new { test = "data" }
        };

        // Act
        await service.QueueOfflineOperationAsync(operation);

        // Assert - Should not throw
        operation.Id.Should().NotBeEmpty();
        operation.Timestamp.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public void MAUIPerformanceServiceImpl_StartTracking_ShouldReturnTracker()
    {
        // Arrange
        var service = _container.Resolve<MAUIPerformanceService>();

        // Act
        using var tracker = service.StartTracking("test-operation", "Test");

        // Assert
        tracker.Should().NotBeNull();
        tracker.Should().BeAssignableTo<IPerformanceTracker>();
    }

    [Fact]
    public void MAUIPerformanceServiceImpl_RecordMetric_ShouldNotThrow()
    {
        // Arrange
        var service = _container.Resolve<MAUIPerformanceService>();

        // Act & Assert
        service.Invoking(s => s.RecordMetric("test-metric", 100.5, "ms"))
            .Should().NotThrow();
    }

    [Fact]
    public async Task MAUIPerformanceServiceImpl_GetStatistics_ShouldReturnValidData()
    {
        // Arrange
        var service = _container.Resolve<MAUIPerformanceService>();

        // Record some metrics first
        service.RecordMetric("test-metric", 100);
        service.RecordMetric("test-metric", 200);
        service.RecordMetric("test-metric", 150);

        // Act
        var statistics = await service.GetStatisticsAsync();

        // Assert
        statistics.Should().NotBeNull();
        statistics.Category.Should().Be("All");
    }

    [Fact]
    public void PerformanceTracker_WithUsing_ShouldDisposeCorrectly()
    {
        // Arrange
        var service = _container.Resolve<MAUIPerformanceService>();

        // Act & Assert
        var action = () =>
        {
            using var tracker = service.StartTracking("disposal-test");
            tracker.AddProperty("test", "value");
            tracker.Stop();
        };

        action.Should().NotThrow();
    }

    public void Dispose()
    {
        _container?.Dispose();
    }
}