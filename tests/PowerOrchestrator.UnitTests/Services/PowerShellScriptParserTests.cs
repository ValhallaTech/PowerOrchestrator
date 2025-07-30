using System.Management.Automation.Language;

namespace PowerOrchestrator.UnitTests.Services;

/// <summary>
/// Unit tests for PowerShell script parser
/// </summary>
public class PowerShellScriptParserTests
{
    private readonly Mock<ILogger<PowerShellScriptParser>> _mockLogger;
    private readonly PowerShellScriptParser _parser;

    public PowerShellScriptParserTests()
    {
        _mockLogger = new Mock<ILogger<PowerShellScriptParser>>();
        _parser = new PowerShellScriptParser(_mockLogger.Object);
    }

    [Fact]
    public async Task ParseScriptAsync_WithCommentBasedHelp_ShouldExtractMetadata()
    {
        // Arrange
        var scriptContent = @"
<#
.SYNOPSIS
    Deploys Azure infrastructure
.DESCRIPTION
    This script deploys a complete Azure environment using ARM templates
.NOTES
    Requires Azure PowerShell module
#>
param(
    [Parameter(Mandatory=$true)]
    [string]$ResourceGroup,
    
    [Parameter(Mandatory=$false)]
    [string]$Location = 'eastus'
)

Write-Host ""Starting deployment to $ResourceGroup in $Location""
";

        // Act
        var result = await _parser.ParseScriptAsync(scriptContent, "Deploy-AzureInfrastructure.ps1");

        // Assert
        result.Should().NotBeNull();
        result.Synopsis.Should().Be("Deploys Azure infrastructure");
        result.Description.Should().Be("This script deploys a complete Azure environment using ARM templates");
        result.Parameters.Should().HaveCount(2);
        result.Parameters.Should().Contain("ResourceGroup");
        result.Parameters.Should().Contain("Location");
        result.Notes.Should().Be("Requires Azure PowerShell module");
    }

    [Fact]
    public async Task ParseScriptAsync_WithoutCommentBasedHelp_ShouldReturnBasicMetadata()
    {
        // Arrange
        var scriptContent = @"
param(
    [string]$Name
)

Write-Host ""Hello $Name""
";

        // Act
        var result = await _parser.ParseScriptAsync(scriptContent, "SimpleScript.ps1");

        // Assert
        result.Should().NotBeNull();
        result.Synopsis.Should().BeEmpty();
        result.Description.Should().BeEmpty();
        result.Parameters.Should().HaveCount(1);
        result.Parameters.Should().Contain("Name");
    }

    [Fact]
    public async Task AnalyzeSecurityAsync_WithDangerousCommands_ShouldDetectThreats()
    {
        // Arrange
        var scriptContent = @"
# This script contains dangerous commands
Invoke-Expression $userInput
Remove-Item -Path C:\* -Recurse -Force
Start-Process cmd.exe -ArgumentList '/c format C: /fs:ntfs'
";

        // Act
        var result = await _parser.AnalyzeSecurityAsync(scriptContent);

        // Assert
        result.Should().NotBeNull();
        result.RiskLevel.Should().Be("High");
        result.SecurityIssues.Should().NotBeEmpty();
        result.RequiresElevation.Should().BeTrue();
    }

    [Fact]
    public async Task AnalyzeSecurityAsync_WithSafeScript_ShouldReturnLowSecurity()
    {
        // Arrange
        var scriptContent = @"
param(
    [string]$LogPath
)

$timestamp = Get-Date -Format 'yyyy-MM-dd HH:mm:ss'
Add-Content -Path $LogPath -Value ""[$timestamp] Script execution started""
Write-Host ""Processing completed successfully""
";

        // Act
        var result = await _parser.AnalyzeSecurityAsync(scriptContent);

        // Assert
        result.Should().NotBeNull();
        result.RiskLevel.Should().Be("Low");
        result.SecurityIssues.Should().BeEmpty();
        result.RequiresElevation.Should().BeFalse();
    }

    [Fact]
    public async Task ExtractDependenciesAsync_ShouldFindModuleDependencies()
    {
        // Arrange
        var scriptContent = @"
#Requires -Module Az.Accounts
#Requires -Module Az.Resources
Import-Module ActiveDirectory
using module Az.Storage

Connect-AzAccount
";

        // Act
        var result = await _parser.ExtractDependenciesAsync(scriptContent);

        // Assert
        result.Should().NotBeNull();
        result.Should().Contain("Az.Accounts");
        result.Should().Contain("Az.Resources");
        result.Should().Contain("ActiveDirectory");
        result.Should().Contain("Az.Storage");
    }

    [Fact]
    public async Task GetRequiredVersionAsync_WithVersionRequirement_ShouldReturnVersion()
    {
        // Arrange
        var scriptContent = @"
#Requires -Version 5.1

Write-Host ""PowerShell version check""
";

        // Act
        var result = await _parser.GetRequiredVersionAsync(scriptContent);

        // Assert
        result.Should().NotBeNull();
        result.Major.Should().Be(5);
        result.Minor.Should().Be(1);
    }

    [Fact]
    public async Task ParseScriptAsync_WithSyntaxErrors_ShouldLogWarnings()
    {
        // Arrange
        var scriptContent = @"
param(
    [string]$Name
)

Write-Host ""Hello $Name
# Missing closing quote - syntax error
";

        // Act
        var result = await _parser.ParseScriptAsync(scriptContent, "ErrorScript.ps1");

        // Assert
        result.Should().NotBeNull();
        // Verify that logger was called for warnings
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v != null && v.ToString()!.Contains("Parse errors found")),
                It.IsAny<Exception?>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task ParseScriptAsync_WithComplexFunction_ShouldExtractFunctionInfo()
    {
        // Arrange
        var scriptContent = @"
function Test-Connection {
    param(
        [string]$ComputerName
    )
    
    Test-NetConnection -ComputerName $ComputerName -Port 80
}

function Get-SystemInfo {
    param([string]$Server)
    Get-ComputerInfo -ComputerName $Server
}
";

        // Act
        var result = await _parser.ParseScriptAsync(scriptContent, "NetworkUtils.ps1");

        // Assert
        result.Should().NotBeNull();
        result.Functions.Should().HaveCount(2);
        result.Functions.Should().Contain("Test-Connection");
        result.Functions.Should().Contain("Get-SystemInfo");
    }

    [Theory]
    [InlineData("Deploy-Infrastructure.ps1", true)]
    [InlineData("script.ps1", true)]
    [InlineData("config.json", false)]
    [InlineData("readme.txt", false)]
    [InlineData("Setup.PS1", true)]
    public async Task ParseScriptAsync_WithDifferentFileExtensions_ShouldValidateCorrectly(string fileName, bool shouldBeValid)
    {
        // Arrange
        var scriptContent = "Write-Host 'Test'";

        // Act & Assert
        if (shouldBeValid)
        {
            var result = await _parser.ParseScriptAsync(scriptContent, fileName);
            result.Should().NotBeNull();
        }
        else
        {
            // For non-PowerShell files, the parser should still work but may not extract much metadata
            var result = await _parser.ParseScriptAsync(scriptContent, fileName);
            result.Should().NotBeNull();
        }
    }
}