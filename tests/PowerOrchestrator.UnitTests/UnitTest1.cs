using FluentAssertions;

namespace PowerOrchestrator.UnitTests;

/// <summary>
/// Validation tests for PowerOrchestrator Phase 0 foundation
/// </summary>
public class FoundationValidationTests
{
    [Fact]
    public void ProjectStructure_ShouldHaveRequiredDirectories()
    {
        // Arrange
        var solutionRoot = GetSolutionRoot();
        var requiredDirectories = new[]
        {
            "src",
            "tests", 
            "docs",
            "scripts",
            "deployment"
        };

        // Act & Assert
        foreach (var directory in requiredDirectories)
        {
            var directoryPath = Path.Combine(solutionRoot, directory);
            Directory.Exists(directoryPath).Should().BeTrue($"Required directory '{directory}' should exist");
        }
    }

    [Fact]
    public void ProjectStructure_ShouldHaveRequiredProjects()
    {
        // Arrange
        var solutionRoot = GetSolutionRoot();
        var requiredProjects = new[]
        {
            "src/PowerOrchestrator.Domain/PowerOrchestrator.Domain.csproj",
            "src/PowerOrchestrator.Application/PowerOrchestrator.Application.csproj",
            "src/PowerOrchestrator.Infrastructure/PowerOrchestrator.Infrastructure.csproj",
            "src/PowerOrchestrator.Identity/PowerOrchestrator.Identity.csproj",
            "src/PowerOrchestrator.API/PowerOrchestrator.API.csproj",
            "src/PowerOrchestrator.MAUI/PowerOrchestrator.MAUI.csproj"
        };

        // Act & Assert
        foreach (var project in requiredProjects)
        {
            var projectPath = Path.Combine(solutionRoot, project);
            File.Exists(projectPath).Should().BeTrue($"Required project '{project}' should exist");
        }
    }

    [Fact]
    public void ConfigurationFiles_ShouldExist()
    {
        // Arrange
        var solutionRoot = GetSolutionRoot();
        var requiredFiles = new[]
        {
            "global.json",
            "Directory.Packages.props",
            "docker-compose.dev.yml",
            "PowerOrchestrator.sln"
        };

        // Act & Assert
        foreach (var file in requiredFiles)
        {
            var filePath = Path.Combine(solutionRoot, file);
            File.Exists(filePath).Should().BeTrue($"Required file '{file}' should exist");
        }
    }

    [Fact]
    public void DatabaseScripts_ShouldExist()
    {
        // Arrange
        var solutionRoot = GetSolutionRoot();
        var databaseScriptPath = Path.Combine(solutionRoot, "scripts", "database", "init.sql");

        // Act & Assert
        File.Exists(databaseScriptPath).Should().BeTrue("Database initialization script should exist");
        
        var scriptContent = File.ReadAllText(databaseScriptPath);
        scriptContent.Should().Contain("powerorchestrator", "Script should contain schema creation");
        scriptContent.Should().Contain("CREATE TABLE", "Script should contain table creation");
    }

    [Fact]
    public void SampleScripts_ShouldExist()
    {
        // Arrange
        var solutionRoot = GetSolutionRoot();
        var sampleScriptsPath = Path.Combine(solutionRoot, "scripts", "sample-scripts");
        var requiredSampleScripts = new[]
        {
            "hello-world.ps1",
            "system-info.ps1"
        };

        // Act & Assert
        Directory.Exists(sampleScriptsPath).Should().BeTrue("Sample scripts directory should exist");
        
        foreach (var script in requiredSampleScripts)
        {
            var scriptPath = Path.Combine(sampleScriptsPath, script);
            File.Exists(scriptPath).Should().BeTrue($"Sample script '{script}' should exist");
        }
    }

    [Fact]
    public void GitHubActionsWorkflow_ShouldExist()
    {
        // Arrange
        var solutionRoot = GetSolutionRoot();
        var workflowPath = Path.Combine(solutionRoot, ".github", "workflows", "ci.yml");

        // Act & Assert
        File.Exists(workflowPath).Should().BeTrue("GitHub Actions CI workflow should exist");
        
        var workflowContent = File.ReadAllText(workflowPath);
        workflowContent.Should().Contain("dotnet build", "Workflow should include build step");
        workflowContent.Should().Contain("dotnet test", "Workflow should include test step");
        workflowContent.Should().Contain("postgres", "Workflow should include PostgreSQL service");
        workflowContent.Should().Contain("redis", "Workflow should include Redis service");
    }

    [Fact]
    public void Documentation_ShouldExist()
    {
        // Arrange
        var solutionRoot = GetSolutionRoot();
        var requiredDocs = new[]
        {
            "README.md",
            "CONTRIBUTING.md"
        };

        // Act & Assert
        foreach (var doc in requiredDocs)
        {
            var docPath = Path.Combine(solutionRoot, doc);
            File.Exists(docPath).Should().BeTrue($"Documentation file '{doc}' should exist");
            
            var content = File.ReadAllText(docPath);
            content.Should().NotBeNullOrWhiteSpace($"Documentation file '{doc}' should have content");
            content.Length.Should().BeGreaterThan(100, $"Documentation file '{doc}' should have substantial content");
        }
    }

    /// <summary>
    /// Gets the solution root directory by traversing up from the test assembly location
    /// </summary>
    private static string GetSolutionRoot()
    {
        var currentDirectory = Directory.GetCurrentDirectory();
        var directory = new DirectoryInfo(currentDirectory);
        
        while (directory != null && !directory.GetFiles("*.sln").Any())
        {
            directory = directory.Parent;
        }
        
        if (directory == null)
        {
            throw new InvalidOperationException("Could not find solution root directory");
        }
        
        return directory.FullName;
    }
}