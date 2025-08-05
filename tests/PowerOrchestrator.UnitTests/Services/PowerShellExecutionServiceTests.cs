using Moq;
using PowerOrchestrator.Application.Interfaces;
using PowerOrchestrator.Application.Interfaces.Repositories;
using PowerOrchestrator.Application.Interfaces.Services;
using PowerOrchestrator.Domain.Entities;
using PowerOrchestrator.Domain.ValueObjects;
using PowerOrchestrator.Infrastructure.Services;
using FluentAssertions;

namespace PowerOrchestrator.UnitTests.Services;

/// <summary>
/// Unit tests for PowerShellExecutionService
/// </summary>
public class PowerShellExecutionServiceTests
{
    private readonly Mock<IUnitOfWork> _mockUnitOfWork;
    private readonly Mock<IPowerShellScriptParser> _mockScriptParser;
    private readonly Mock<IExecutionNotificationService> _mockNotificationService;
    private readonly Mock<IExecutionRepository> _mockExecutionRepository;
    private readonly Mock<IScriptRepository> _mockScriptRepository;
    private readonly PowerShellExecutionService _service;

    public PowerShellExecutionServiceTests()
    {
        _mockUnitOfWork = new Mock<IUnitOfWork>();
        _mockScriptParser = new Mock<IPowerShellScriptParser>();
        _mockNotificationService = new Mock<IExecutionNotificationService>();
        _mockExecutionRepository = new Mock<IExecutionRepository>();
        _mockScriptRepository = new Mock<IScriptRepository>();

        // Setup options
        var options = new PowerShellExecutionOptions
        {
            UseConstrainedLanguageMode = true,
            MaxExecutionTimeSeconds = 3600,
            MaxConcurrentExecutions = 50,
            MaxMemoryUsageMB = 500
        };

        // Setup unit of work
        _mockUnitOfWork.Setup(x => x.Executions).Returns(_mockExecutionRepository.Object);
        _mockUnitOfWork.Setup(x => x.Scripts).Returns(_mockScriptRepository.Object);

        _service = new PowerShellExecutionService(
            _mockUnitOfWork.Object,
            _mockScriptParser.Object,
            _mockNotificationService.Object,
            options);
    }

    [Fact]
    public async Task ExecuteScriptAsync_WithValidScriptId_ShouldReturnExecutionId()
    {
        // Arrange
        var scriptId = Guid.NewGuid();
        var script = new Script
        {
            Id = scriptId,
            Name = "Test Script",
            Content = "Write-Host 'Hello World'"
        };

        _mockScriptRepository.Setup(x => x.GetByIdAsync(scriptId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(script);

        _mockExecutionRepository.Setup(x => x.AddAsync(It.IsAny<Execution>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Execution execution, CancellationToken ct) => 
            {
                execution.Id = Guid.NewGuid();
                return execution;
            });

        _mockUnitOfWork.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        // Act
        var result = await _service.ExecuteScriptAsync(scriptId);

        // Assert
        result.Should().NotBeEmpty();
        _mockScriptRepository.Verify(x => x.GetByIdAsync(scriptId, It.IsAny<CancellationToken>()), Times.Once);
        _mockExecutionRepository.Verify(x => x.AddAsync(It.IsAny<Execution>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ExecuteScriptAsync_WithInvalidScriptId_ShouldThrowArgumentException()
    {
        // Arrange
        var scriptId = Guid.NewGuid();
        _mockScriptRepository.Setup(x => x.GetByIdAsync(scriptId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Script?)null);

        // Act & Assert
        await _service.Invoking(s => s.ExecuteScriptAsync(scriptId))
            .Should().ThrowAsync<ArgumentException>()
            .WithMessage($"Script with ID {scriptId} not found*");
    }

    [Fact]
    public async Task ExecuteScriptContentAsync_WithEmptyContent_ShouldThrowArgumentException()
    {
        // Act & Assert
        await _service.Invoking(s => s.ExecuteScriptContentAsync(string.Empty))
            .Should().ThrowAsync<ArgumentException>()
            .WithMessage("Script content cannot be null or empty*");
    }

    [Fact]
    public async Task ExecuteScriptContentAsync_WithValidContent_ShouldReturnExecutionId()
    {
        // Arrange
        var scriptContent = "Write-Host 'Hello World'";

        _mockExecutionRepository.Setup(x => x.AddAsync(It.IsAny<Execution>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Execution execution, CancellationToken ct) => 
            {
                execution.Id = Guid.NewGuid();
                return execution;
            });

        _mockUnitOfWork.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        // Act
        var result = await _service.ExecuteScriptContentAsync(scriptContent);

        // Assert
        result.Should().NotBeEmpty();
        _mockExecutionRepository.Verify(x => x.AddAsync(It.IsAny<Execution>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetExecutionStatusAsync_WithValidExecutionId_ShouldReturnExecution()
    {
        // Arrange
        var executionId = Guid.NewGuid();
        var execution = new Execution
        {
            Id = executionId,
            Status = ExecutionStatus.Running
        };

        _mockExecutionRepository.Setup(x => x.GetByIdAsync(executionId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(execution);

        // Act
        var result = await _service.GetExecutionStatusAsync(executionId);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(executionId);
        result.Status.Should().Be(ExecutionStatus.Running);
    }

    [Fact]
    public async Task ValidateExecutionAsync_WithValidScript_ShouldReturnValidResult()
    {
        // Arrange
        var scriptId = Guid.NewGuid();
        var script = new Script
        {
            Id = scriptId,
            Name = "Test Script",
            Content = "Write-Host 'Hello World'"
        };

        var metadata = new ScriptMetadata();
        var securityAnalysis = new SecurityAnalysis { RiskLevel = "Low" };
        var dependencies = new List<string>();

        _mockScriptRepository.Setup(x => x.GetByIdAsync(scriptId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(script);

        _mockScriptParser.Setup(x => x.ParseScriptAsync(script.Content, script.Name, It.IsAny<CancellationToken>()))
            .ReturnsAsync(metadata);

        _mockScriptParser.Setup(x => x.AnalyzeSecurityAsync(script.Content, It.IsAny<CancellationToken>()))
            .ReturnsAsync(securityAnalysis);

        _mockScriptParser.Setup(x => x.ExtractDependenciesAsync(script.Content, It.IsAny<CancellationToken>()))
            .ReturnsAsync(dependencies);

        // Act
        var result = await _service.ValidateExecutionAsync(scriptId);

        // Assert
        result.Should().NotBeNull();
        result.IsValid.Should().BeTrue();
        result.SecurityRiskLevel.Should().Be("Low");
    }

    [Fact]
    public async Task ValidateExecutionAsync_WithInvalidScript_ShouldReturnInvalidResult()
    {
        // Arrange
        var scriptId = Guid.NewGuid();
        _mockScriptRepository.Setup(x => x.GetByIdAsync(scriptId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Script?)null);

        // Act
        var result = await _service.ValidateExecutionAsync(scriptId);

        // Assert
        result.Should().NotBeNull();
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain($"Script with ID {scriptId} not found");
    }

    [Fact]
    public async Task GetRunningExecutionsAsync_ShouldCallRepository()
    {
        // Arrange
        var runningExecutions = new List<Execution>
        {
            new() { Id = Guid.NewGuid(), Status = ExecutionStatus.Running }
        };

        _mockExecutionRepository.Setup(x => x.GetRunningExecutionsAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(runningExecutions);

        // Act
        var result = await _service.GetRunningExecutionsAsync();

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(1);
        _mockExecutionRepository.Verify(x => x.GetRunningExecutionsAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetExecutionMetricsAsync_WithValidExecution_ShouldReturnMetrics()
    {
        // Arrange
        var executionId = Guid.NewGuid();
        var execution = new Execution
        {
            Id = executionId,
            ScriptId = Guid.NewGuid(),
            StartedAt = DateTime.UtcNow.AddMinutes(-5),
            CompletedAt = DateTime.UtcNow,
            DurationMs = 5000,
            PowerShellVersion = "7.4.6",
            ExecutedOn = "TestMachine",
            Output = "Hello World",
            ErrorOutput = null
        };

        _mockExecutionRepository.Setup(x => x.GetByIdAsync(executionId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(execution);

        // Act
        var result = await _service.GetExecutionMetricsAsync(executionId);

        // Assert
        result.Should().NotBeNull();
        result!.ExecutionId.Should().Be(executionId);
        result.DurationMs.Should().Be(5000);
        result.PowerShellVersion.Should().Be("7.4.6");
        result.HostMachine.Should().Be("TestMachine");
    }

    [Fact]
    public async Task GetExecutionMetricsAsync_WithInvalidExecution_ShouldReturnNull()
    {
        // Arrange
        var executionId = Guid.NewGuid();
        _mockExecutionRepository.Setup(x => x.GetByIdAsync(executionId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Execution?)null);

        // Act
        var result = await _service.GetExecutionMetricsAsync(executionId);

        // Assert
        result.Should().BeNull();
    }
}