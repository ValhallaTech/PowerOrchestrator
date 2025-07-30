using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Running;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using PowerOrchestrator.Application.Interfaces;
using PowerOrchestrator.Application.Interfaces.Services;
using PowerOrchestrator.Domain.ValueObjects;
using PowerOrchestrator.Infrastructure.Configuration;
using PowerOrchestrator.Infrastructure.Services;

namespace PowerOrchestrator.LoadTests.Performance;

/// <summary>
/// Performance tests for GitHub synchronization operations
/// </summary>
[MemoryDiagnoser]
[SimpleJob]
[RPlotExporter]
public class GitHubSyncPerformanceTests
{
    private Mock<ILogger<RepositorySyncService>> _mockLogger;
    private Mock<IGitHubService> _mockGitHubService;
    private Mock<IPowerShellScriptParser> _mockParser;
    private Mock<IUnitOfWork> _mockUnitOfWork;
    private Mock<IRepositoryManager> _mockRepositoryManager;
    private RepositorySyncService _syncService;
    private List<GitHubFile> _smallRepositoryFiles;
    private List<GitHubFile> _largeRepositoryFiles;

    [GlobalSetup]
    public void Setup()
    {
        _mockLogger = new Mock<ILogger<RepositorySyncService>>();
        _mockGitHubService = new Mock<IGitHubService>();
        _mockParser = new Mock<IPowerShellScriptParser>();
        _mockUnitOfWork = new Mock<IUnitOfWork>();
        _mockRepositoryManager = new Mock<IRepositoryManager>();

        // Create test data for different repository sizes
        _smallRepositoryFiles = GenerateTestFiles(10);
        _largeRepositoryFiles = GenerateTestFiles(100);

        // Setup mocks with realistic behavior
        SetupMocks();

        _syncService = new RepositorySyncService(
            _mockLogger.Object,
            _mockUnitOfWork.Object,
            _mockGitHubService.Object,
            _mockParser.Object,
            _mockRepositoryManager.Object);
    }

    [Benchmark]
    [Arguments(10)]
    [Arguments(50)]
    [Arguments(100)]
    public async Task SyncRepository_WithDifferentFileCounts(int fileCount)
    {
        // Arrange
        var files = GenerateTestFiles(fileCount);
        var repositoryName = $"test/repo-{fileCount}-files";

        _mockGitHubService.Setup(x => x.GetScriptFilesAsync("test", $"repo-{fileCount}-files", null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(files);

        // Act
        await _syncService.SynchronizeRepositoryAsync(repositoryName);
    }

    [Benchmark]
    public async Task ParseMultipleScripts_Small()
    {
        // Test parsing 10 PowerShell scripts
        var tasks = new List<Task>();
        for (int i = 0; i < 10; i++)
        {
            var script = GenerateTestScript(i);
            tasks.Add(_mockParser.Object.ParseScriptAsync(script, $"script{i}.ps1"));
        }

        await Task.WhenAll(tasks);
    }

    [Benchmark]
    public async Task ParseMultipleScripts_Large()
    {
        // Test parsing 100 PowerShell scripts
        var tasks = new List<Task>();
        for (int i = 0; i < 100; i++)
        {
            var script = GenerateTestScript(i);
            tasks.Add(_mockParser.Object.ParseScriptAsync(script, $"script{i}.ps1"));
        }

        await Task.WhenAll(tasks);
    }

    [Benchmark]
    public async Task ConcurrentSyncOperations()
    {
        // Test 5 concurrent synchronization operations
        var tasks = new List<Task>();
        for (int i = 0; i < 5; i++)
        {
            var repositoryName = $"test/concurrent-repo-{i}";
            tasks.Add(_syncService.SynchronizeRepositoryAsync(repositoryName));
        }

        await Task.WhenAll(tasks);
    }

    [Benchmark]
    [Arguments(100)]
    [Arguments(1000)]
    [Arguments(5000)]
    public void RateLimitServicePerformance(int requestCount)
    {
        // Test rate limiting performance with different request volumes
        var mockLogger = new Mock<ILogger<GitHubRateLimitService>>();
        var mockOptions = new Mock<IOptions<GitHubOptions>>();
        mockOptions.Setup(x => x.Value).Returns(new GitHubOptions());
        var rateLimitService = new GitHubRateLimitService(mockLogger.Object, mockOptions.Object);

        // Simulate multiple rate limit checks
        var tasks = new List<Task>();
        for (int i = 0; i < requestCount; i++)
        {
            tasks.Add(rateLimitService.WaitForRateLimitAsync());
        }

        Task.WhenAll(tasks).Wait();
    }

    [Benchmark]
    public async Task MemoryUsageDuringSync()
    {
        // Test memory usage during large repository sync
        var files = GenerateTestFiles(500); // Large repository

        _mockGitHubService.Setup(x => x.GetScriptFilesAsync("test", "large-repo", null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(files);

        await _syncService.SynchronizeRepositoryAsync("test/large-repo");
    }

    private List<GitHubFile> GenerateTestFiles(int count)
    {
        var files = new List<GitHubFile>();
        for (int i = 0; i < count; i++)
        {
            files.Add(new GitHubFile
            {
                Path = $"scripts/script{i}.ps1",
                Name = $"script{i}.ps1",
                Content = GenerateTestScript(i),
                Sha = $"sha{i:D10}",
                Size = 1000 + i,
                Encoding = "utf-8"
            });
        }
        return files;
    }

    private string GenerateTestScript(int index)
    {
        return $@"
<#
.SYNOPSIS
    Test script {index}
.DESCRIPTION
    This is a test PowerShell script for performance testing
.PARAMETER Name
    Test parameter
#>
param(
    [string]$Name = 'Test{index}'
)

Write-Host ""Processing $Name""
Get-Process | Where-Object {{ $_.Name -like '*test*' }}

function Test-Function{index} {{
    param([string]$Input)
    return ""Processed: $Input""
}}

# Simulate some processing
1..10 | ForEach-Object {{
    Start-Sleep -Milliseconds 1
    Write-Output ""Step $_""
}}
";
    }

    private void SetupMocks()
    {
        // Setup realistic parser behavior
        _mockParser.Setup(x => x.ParseScriptAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .Returns<string, string, CancellationToken>((content, fileName, token) =>
            {
                // Simulate parsing time
                Task.Delay(5, token).Wait(token); // 5ms per script
                return Task.FromResult(new ScriptMetadata
                {
                    Synopsis = "Test script",
                    Description = "Performance test script",
                    Parameters = new List<string> { "Name" },
                    Functions = new List<string> { $"Test-Function{fileName.GetHashCode()}" }
                });
            });

        _mockParser.Setup(x => x.AnalyzeSecurityAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .Returns<string, CancellationToken>((content, token) =>
            {
                // Simulate security analysis time
                Task.Delay(2, token).Wait(token); // 2ms per script
                return Task.FromResult(new SecurityAnalysis
                {
                    RiskLevel = "Low",
                    SecurityIssues = new List<string>(),
                    RequiresElevation = false
                });
            });

        // Setup UnitOfWork
        _mockUnitOfWork.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);
    }
}

/// <summary>
/// Benchmarks for rate limiting performance
/// </summary>
[MemoryDiagnoser]
[SimpleJob]
public class GitHubRateLimitBenchmarks
{
    private GitHubRateLimitService _rateLimitService;

    [GlobalSetup]
    public void Setup()
    {
        var mockLogger = new Mock<ILogger<GitHubRateLimitService>>();
        var mockOptions = new Mock<IOptions<GitHubOptions>>();
        mockOptions.Setup(x => x.Value).Returns(new GitHubOptions());
        _rateLimitService = new GitHubRateLimitService(mockLogger.Object, mockOptions.Object);
    }

    [Benchmark]
    [Arguments(100)]
    [Arguments(1000)]
    [Arguments(5000)]
    public async Task ConcurrentRateLimitChecks(int concurrency)
    {
        var tasks = new List<Task>();
        for (int i = 0; i < concurrency; i++)
        {
            tasks.Add(_rateLimitService.WaitForRateLimitAsync());
        }

        await Task.WhenAll(tasks);
    }

    [Benchmark]
    public void UpdateRateLimitInfo()
    {
        // Test the performance of updating rate limit information
        for (int i = 0; i < 1000; i++)
        {
            _rateLimitService.UpdateRateLimitInfo(5000 - i, 5000, DateTime.UtcNow.AddMinutes(60));
        }
    }
}

/// <summary>
/// Program entry point for running benchmarks
/// To run manually: dotnet run --project PowerOrchestrator.LoadTests -c Release --framework net8.0
/// </summary>
internal class GitHubPerformanceProgram
{
    // Entry point removed to avoid conflicts with test runner
    // Use: dotnet run with BenchmarkDotNet.Tool for execution
}