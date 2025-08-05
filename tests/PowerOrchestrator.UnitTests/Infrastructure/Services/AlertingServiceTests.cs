using FluentAssertions;
using Microsoft.Extensions.Options;
using Moq;
using PowerOrchestrator.Application.Interfaces.Repositories;
using PowerOrchestrator.Application.Interfaces.Services;
using PowerOrchestrator.Domain.Entities;
using PowerOrchestrator.Infrastructure.Configuration;
using PowerOrchestrator.Infrastructure.Services;
using Serilog;

namespace PowerOrchestrator.UnitTests.Infrastructure.Services;

/// <summary>
/// Unit tests for AlertingService
/// </summary>
public class AlertingServiceTests : IDisposable
{
    private readonly AlertingService _service;
    private readonly Mock<IAlertConfigurationRepository> _alertConfigRepositoryMock;
    private readonly Mock<IAlertInstanceRepository> _alertInstanceRepositoryMock;
    private readonly Mock<IPerformanceMonitoringService> _performanceMonitoringMock;
    private readonly Mock<INotificationService> _notificationServiceMock;
    private readonly AlertingOptions _options;

    public AlertingServiceTests()
    {
        // Configure Serilog for testing
        Log.Logger = new LoggerConfiguration()
            .WriteTo.Console()
            .CreateLogger();

        _options = new AlertingOptions
        {
            Enabled = false, // Disable timers for testing
            ProcessingIntervalSeconds = 5,
            MaxProcessingTimeSeconds = 15
        };

        var optionsMock = new Mock<IOptions<AlertingOptions>>();
        optionsMock.Setup(o => o.Value).Returns(_options);

        _alertConfigRepositoryMock = new Mock<IAlertConfigurationRepository>();
        _alertInstanceRepositoryMock = new Mock<IAlertInstanceRepository>();
        _performanceMonitoringMock = new Mock<IPerformanceMonitoringService>();
        _notificationServiceMock = new Mock<INotificationService>();

        _service = new AlertingService(
            optionsMock.Object,
            _alertConfigRepositoryMock.Object,
            _alertInstanceRepositoryMock.Object,
            _performanceMonitoringMock.Object,
            _notificationServiceMock.Object);
    }

    [Fact]
    public async Task CreateAlertAsync_ShouldCreateAlert()
    {
        // Arrange
        var alertConfig = new AlertConfiguration
        {
            Name = "Test Alert",
            MetricName = "cpu.usage",
            Condition = "GreaterThan",
            ThresholdValue = 80.0,
            Severity = "High",
            IsEnabled = true,
            NotificationChannels = new List<string> { "webhook" }
        };

        _alertConfigRepositoryMock
            .Setup(r => r.CreateAsync(It.IsAny<AlertConfiguration>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _service.CreateAlertAsync(alertConfig);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().NotBe(Guid.Empty);
        result.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
        result.ModifiedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));

        _alertConfigRepositoryMock.Verify(r => r.CreateAsync(It.IsAny<AlertConfiguration>()), Times.Once);
    }

    [Fact]
    public async Task UpdateAlertAsync_ShouldUpdateAlert()
    {
        // Arrange
        var alertConfig = new AlertConfiguration
        {
            Id = Guid.NewGuid(),
            Name = "Updated Alert",
            MetricName = "memory.usage",
            Condition = "LessThan",
            ThresholdValue = 20.0,
            CreatedAt = DateTime.UtcNow.AddDays(-1)
        };

        _alertConfigRepositoryMock
            .Setup(r => r.UpdateAsync(It.IsAny<AlertConfiguration>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _service.UpdateAlertAsync(alertConfig);

        // Assert
        result.Should().NotBeNull();
        result.ModifiedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
        result.CreatedAt.Should().BeBefore(result.ModifiedAt);

        _alertConfigRepositoryMock.Verify(r => r.UpdateAsync(It.IsAny<AlertConfiguration>()), Times.Once);
    }

    [Fact]
    public async Task GetAlertConfigurationsAsync_ShouldReturnConfigurations()
    {
        // Arrange
        var configurations = new List<AlertConfiguration>
        {
            new() { Id = Guid.NewGuid(), Name = "Alert 1" },
            new() { Id = Guid.NewGuid(), Name = "Alert 2" }
        };

        _alertConfigRepositoryMock
            .Setup(r => r.GetAllAsync())
            .ReturnsAsync(configurations);

        // Act
        var result = await _service.GetAlertConfigurationsAsync();

        // Assert
        result.Should().HaveCount(2);
        result.Should().BeEquivalentTo(configurations);
    }

    [Fact]
    public async Task AcknowledgeAlertAsync_ShouldUpdateAlertState()
    {
        // Arrange
        var alertId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var alertInstance = new AlertInstance
        {
            Id = alertId,
            State = "Triggered",
            TriggeredAt = DateTime.UtcNow.AddMinutes(-5)
        };

        _alertInstanceRepositoryMock
            .Setup(r => r.GetByIdAsync(alertId))
            .ReturnsAsync(alertInstance);

        _alertInstanceRepositoryMock
            .Setup(r => r.UpdateAsync(It.IsAny<AlertInstance>()))
            .Returns(Task.CompletedTask);

        // Act
        await _service.AcknowledgeAlertAsync(alertId, userId);

        // Assert
        alertInstance.State.Should().Be("Acknowledged");
        alertInstance.AcknowledgedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
        alertInstance.AcknowledgedBy.Should().Be(userId);

        _alertInstanceRepositoryMock.Verify(r => r.UpdateAsync(alertInstance), Times.Once);
    }

    [Fact]
    public async Task ResolveAlertAsync_ShouldUpdateAlertState()
    {
        // Arrange
        var alertId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var alertInstance = new AlertInstance
        {
            Id = alertId,
            State = "Acknowledged",
            TriggeredAt = DateTime.UtcNow.AddMinutes(-10),
            AcknowledgedAt = DateTime.UtcNow.AddMinutes(-5)
        };

        _alertInstanceRepositoryMock
            .Setup(r => r.GetByIdAsync(alertId))
            .ReturnsAsync(alertInstance);

        _alertInstanceRepositoryMock
            .Setup(r => r.UpdateAsync(It.IsAny<AlertInstance>()))
            .Returns(Task.CompletedTask);

        // Act
        await _service.ResolveAlertAsync(alertId, userId);

        // Assert
        alertInstance.State.Should().Be("Resolved");
        alertInstance.ResolvedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
        alertInstance.ResolvedBy.Should().Be(userId);

        _alertInstanceRepositoryMock.Verify(r => r.UpdateAsync(alertInstance), Times.Once);
    }

    [Fact]
    public async Task GetActiveAlertsAsync_ShouldReturnActiveAlerts()
    {
        // Arrange
        var activeAlerts = new List<AlertInstance>
        {
            new() { Id = Guid.NewGuid(), State = "Triggered" },
            new() { Id = Guid.NewGuid(), State = "Acknowledged" }
        };

        _alertInstanceRepositoryMock
            .Setup(r => r.GetActiveAlertsAsync())
            .ReturnsAsync(activeAlerts);

        // Act
        var result = await _service.GetActiveAlertsAsync();

        // Assert
        result.Should().HaveCount(2);
        result.Should().BeEquivalentTo(activeAlerts);
    }

    public void Dispose()
    {
        _service?.Dispose();
        Log.CloseAndFlush();
    }
}