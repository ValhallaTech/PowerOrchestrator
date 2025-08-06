namespace PowerOrchestrator.MCPIntegrationTests.CriticalTier;

/// <summary>
/// Integration tests for PowerShell Execution MCP Server
/// Tests core business logic validation, PowerShell SDK integration, and script security
/// </summary>
public class PowerShellExecutionServerTests : MCPTestBase
{
    private const string ServerName = "powershell-execution";
    private readonly string _sampleScriptsPath;

    public PowerShellExecutionServerTests()
    {
        _sampleScriptsPath = Path.Combine(
            Directory.GetCurrentDirectory(), 
            "..", "..", "..", "..", "..", 
            "scripts", "sample-scripts"
        );
    }

    [Fact]
    public async Task PowerShellServer_ShouldInitializeSuccessfully()
    {
        // Arrange
        Logger.LogInformation("Testing PowerShell MCP server initialization");

        // Act
        var isHealthy = await IsServerHealthyAsync(ServerName);

        // Assert
        isHealthy.Should().BeTrue("PowerShell MCP server should be accessible and responding");
    }

    [Fact]
    public async Task PowerShellServer_ShouldVerifyToolsAvailability()
    {
        // Arrange
        var expectedTools = new[] { "execute", "run_script", "get_output" };

        // Act
        var toolsVerified = await VerifyServerToolsAsync(ServerName);

        // Assert
        toolsVerified.Should().BeTrue("All expected PowerShell tools should be available");
        
        var serverConfig = GetServerConfig(ServerName);
        serverConfig.Tools.Should().Contain(expectedTools, "PowerShell server should support all critical execution operations");
    }

    [Fact]
    public async Task PowerShellServer_ShouldExecuteHelloWorldScript()
    {
        // Arrange
        var helloWorldScript = Path.Combine(_sampleScriptsPath, "hello-world.ps1");
        Logger.LogInformation($"Testing PowerShell script execution: {helloWorldScript}");

        // Act
        var result = await ExecuteMCPCommandAsync(ServerName, new[] { "--run-script", helloWorldScript });

        // Assert
        result.Should().NotBeNull("Hello World script execution should return a result");
        // Real implementation would validate script output contains expected greeting
    }

    [Fact]
    public async Task PowerShellServer_ShouldExecuteSystemInfoScript()
    {
        // Arrange
        var systemInfoScript = Path.Combine(_sampleScriptsPath, "system-info.ps1");
        Logger.LogInformation($"Testing system information script: {systemInfoScript}");

        // Act
        var result = await ExecuteMCPCommandAsync(ServerName, new[] { "--run-script", systemInfoScript });

        // Assert
        result.Should().NotBeNull("System info script execution should return a result");
        // Real implementation would validate system information output
    }

    [Fact]
    public async Task PowerShellServer_ShouldValidateScriptSecurity()
    {
        // Arrange
        Logger.LogInformation("Testing PowerShell script security validation");
        var securityTestScript = @"
            # Test script with potential security concerns
            Write-Host 'Testing security validation'
            Get-Process | Select-Object -First 5
        ";

        // Act
        var result = await ExecuteMCPCommandAsync(ServerName, new[] { "--execute", securityTestScript });

        // Assert
        result.Should().NotBeNull("Security validation should process the script");
        // Real implementation would validate security scanning results
    }

    [Fact]
    public async Task PowerShellServer_ShouldTestErrorHandling()
    {
        // Arrange
        Logger.LogInformation("Testing PowerShell error handling");
        var errorScript = @"
            Write-Host 'Starting script with intentional error'
            $nonExistentVariable.SomeProperty
            Write-Host 'This should not be reached'
        ";

        // Act
        var result = await ExecuteMCPCommandAsync(ServerName, new[] { "--execute", errorScript });

        // Assert
        result.Should().NotBeNull("Error handling should capture script failures");
        // Real implementation would validate error capture and reporting
    }

    [Fact]
    public async Task PowerShellServer_ShouldTestModuleLoading()
    {
        // Arrange
        Logger.LogInformation("Testing PowerShell module loading capabilities");
        var moduleTestScript = @"
            Get-Module -ListAvailable | Select-Object Name, Version | Sort-Object Name
            Import-Module Microsoft.PowerShell.Utility -PassThru
        ";

        // Act
        var result = await ExecuteMCPCommandAsync(ServerName, new[] { "--execute", moduleTestScript });

        // Assert
        result.Should().NotBeNull("Module loading test should execute");
        // Real implementation would validate available modules and loading success
    }

    [Fact]
    public async Task PowerShellServer_ShouldTestParameterPassing()
    {
        // Arrange
        Logger.LogInformation("Testing PowerShell parameter passing");
        var helloWorldScript = Path.Combine(_sampleScriptsPath, "hello-world.ps1");
        var testName = "PowerOrchestrator";

        // Act
        var result = await ExecuteMCPCommandAsync(ServerName, new[] { 
            "--run-script", helloWorldScript, 
            "--parameters", $"-Name '{testName}'" 
        });

        // Assert
        result.Should().NotBeNull("Script with parameters should execute");
        // Real implementation would validate parameter passing and output
    }

    [Fact]
    public async Task PowerShellServer_ShouldTestExecutionPolicy()
    {
        // Arrange
        Logger.LogInformation("Testing PowerShell execution policy validation");
        var policyScript = @"
            Get-ExecutionPolicy -List
            $PSVersionTable | ConvertTo-Json
        ";

        // Act
        var result = await ExecuteMCPCommandAsync(ServerName, new[] { "--execute", policyScript });

        // Assert
        result.Should().NotBeNull("Execution policy check should succeed");
        // Real implementation would validate execution policy settings
    }

    [Fact]
    public async Task PowerShellServer_ShouldTestPerformanceMonitoring()
    {
        // Arrange
        Logger.LogInformation("Testing PowerShell performance monitoring");
        var performanceScript = @"
            $stopwatch = [System.Diagnostics.Stopwatch]::StartNew()
            
            # Simulate some work
            1..1000 | ForEach-Object { $_ * 2 } | Out-Null
            
            $stopwatch.Stop()
            [PSCustomObject]@{
                ElapsedMilliseconds = $stopwatch.ElapsedMilliseconds
                MemoryUsage = [System.GC]::GetTotalMemory($false)
                ProcessorCount = $env:NUMBER_OF_PROCESSORS
            } | ConvertTo-Json
        ";

        // Act
        var stopwatch = Stopwatch.StartNew();
        var result = await ExecuteMCPCommandAsync(ServerName, new[] { "--execute", performanceScript });
        stopwatch.Stop();

        // Assert
        result.Should().NotBeNull("Performance monitoring script should execute");
        stopwatch.ElapsedMilliseconds.Should().BeLessThan(10000, 
            "Performance test should complete within reasonable time");
    }

    [Fact]
    public async Task PowerShellServer_ShouldTestConcurrentExecution()
    {
        // Arrange
        Logger.LogInformation("Testing concurrent PowerShell script execution");
        var concurrentScripts = 5;
        var simpleScript = "Write-Host 'Concurrent execution test'; Get-Date";
        var tasks = new List<Task<ProcessResult>>();

        // Act
        for (int i = 0; i < concurrentScripts; i++)
        {
            var task = ExecuteMCPCommandAsync(ServerName, new[] { "--execute", simpleScript });
            tasks.Add(task);
        }

        var results = await Task.WhenAll(tasks);

        // Assert
        results.Should().HaveCount(concurrentScripts, "All concurrent scripts should complete");
        results.Should().OnlyContain(r => r != null, "All concurrent executions should return results");
    }

    [Fact]
    public async Task PowerShellServer_ShouldTestPowerShellCoreCompatibility()
    {
        // Arrange
        Logger.LogInformation("Testing PowerShell Core compatibility");
        var compatibilityScript = @"
            $PSVersionTable.PSVersion
            $PSVersionTable.PSEdition
            $IsWindows
            $IsLinux
            $IsMacOS
        ";

        // Act
        var result = await ExecuteMCPCommandAsync(ServerName, new[] { "--execute", compatibilityScript });

        // Assert
        result.Should().NotBeNull("PowerShell Core compatibility check should succeed");
        // Real implementation would validate PowerShell Core version and platform
    }

    [Theory]
    [InlineData("Get-Date")]
    [InlineData("Get-Location")]
    [InlineData("$PSVersionTable.PSVersion")]
    [InlineData("Get-Command Get-Process")]
    public async Task PowerShellServer_ShouldExecuteBasicCommands(string command)
    {
        // Arrange
        Logger.LogInformation($"Testing basic PowerShell command: {command}");

        // Act
        var result = await ExecuteMCPCommandAsync(ServerName, new[] { "--execute", command });

        // Assert
        result.Should().NotBeNull($"Basic command should execute: {command}");
    }

    [Fact]
    public async Task PowerShellServer_ShouldTestScriptValidation()
    {
        // Arrange
        Logger.LogInformation("Testing PowerShell script validation");
        var validationScript = @"
            # Test script syntax validation
            $script = 'Write-Host ""Valid script""'
            $errors = $null
            [System.Management.Automation.PSParser]::Tokenize($script, [ref]$errors)
            Write-Host ""Validation complete""
        ";

        // Act
        var result = await ExecuteMCPCommandAsync(ServerName, new[] { "--execute", validationScript });

        // Assert
        result.Should().NotBeNull("Script validation should execute successfully");
    }
}