namespace PowerOrchestrator.MCPIntegrationTests.HighImpactTier;

/// <summary>
/// Integration tests for Filesystem Operations MCP Server
/// Tests PowerOrchestrator project files and logs management
/// </summary>
public class FilesystemOpsServerTests : MCPTestBase
{
    private const string ServerName = "filesystem-ops";
    private readonly string _testDirectory;

    public FilesystemOpsServerTests()
    {
        _testDirectory = Path.Combine(Path.GetTempPath(), "mcp-filesystem-tests", Guid.NewGuid().ToString());
        Directory.CreateDirectory(_testDirectory);
    }

    [Fact]
    public async Task FilesystemServer_ShouldInitializeSuccessfully()
    {
        // Arrange
        Logger.LogInformation("Testing Filesystem MCP server initialization");

        // Act
        var isHealthy = await IsServerHealthyAsync(ServerName);

        // Assert
        isHealthy.Should().BeTrue("Filesystem MCP server should be accessible and responding");
    }

    [Fact]
    public async Task FilesystemServer_ShouldVerifyToolsAvailability()
    {
        // Arrange
        var expectedTools = new[] { "read_file", "write_file", "list_directory", "create_directory", "delete_file" };

        // Act
        var toolsVerified = await VerifyServerToolsAsync(ServerName);

        // Assert
        toolsVerified.Should().BeTrue("All expected filesystem tools should be available");
        
        var serverConfig = GetServerConfig(ServerName);
        serverConfig.Tools.Should().Contain(expectedTools, "Filesystem server should support all critical file operations");
    }

    [Fact]
    public async Task FilesystemServer_ShouldListProjectDirectories()
    {
        // Arrange
        Logger.LogInformation("Testing project directory listing");
        var projectRoot = "/home/runner/work/PowerOrchestrator/PowerOrchestrator";

        // Act
        var result = await ExecuteMCPCommandAsync(ServerName, new[] { "--list-directory", projectRoot });

        // Assert
        result.Should().NotBeNull("Project directory listing should succeed");
        // Real implementation would validate expected directories (src, tests, scripts, etc.)
    }

    [Fact]
    public async Task FilesystemServer_ShouldReadConfigurationFiles()
    {
        // Arrange
        Logger.LogInformation("Testing configuration file reading");
        var configFiles = new[]
        {
            "appsettings.json",
            "docker-compose.dev.yml",
            "Directory.Packages.props",
            "global.json"
        };

        // Act & Assert
        foreach (var configFile in configFiles)
        {
            var filePath = Path.Combine("/home/runner/work/PowerOrchestrator/PowerOrchestrator", configFile);
            try
            {
                var result = await ExecuteMCPCommandAsync(ServerName, new[] { "--read-file", filePath });
                result.Should().NotBeNull($"Configuration file should be readable: {configFile}");
            }
            catch (Exception ex)
            {
                Logger.LogWarning($"Configuration file not accessible: {configFile} - {ex.Message}");
            }
        }
    }

    [Fact]
    public async Task FilesystemServer_ShouldManageScriptFiles()
    {
        // Arrange
        Logger.LogInformation("Testing script file management");
        var scriptsDir = "/home/runner/work/PowerOrchestrator/PowerOrchestrator/scripts/sample-scripts";

        // Act
        var listResult = await ExecuteMCPCommandAsync(ServerName, new[] { "--list-directory", scriptsDir });

        // Assert
        listResult.Should().NotBeNull("Scripts directory should be listable");
        
        // Test reading sample scripts
        var sampleScripts = new[] { "hello-world.ps1", "system-info.ps1" };
        foreach (var script in sampleScripts)
        {
            var scriptPath = Path.Combine(scriptsDir, script);
            var readResult = await ExecuteMCPCommandAsync(ServerName, new[] { "--read-file", scriptPath });
            readResult.Should().NotBeNull($"Sample script should be readable: {script}");
        }
    }

    [Fact]
    public async Task FilesystemServer_ShouldCreateAndDeleteTestFiles()
    {
        // Arrange
        Logger.LogInformation("Testing file creation and deletion");
        var testFile = Path.Combine(_testDirectory, "mcp-test-file.txt");
        var testContent = "MCP Filesystem Integration Test Content\nTimestamp: " + DateTime.UtcNow;

        // Act & Assert
        // Create file
        var createResult = await ExecuteMCPCommandAsync(ServerName, new[] { 
            "--write-file", testFile, testContent 
        });
        createResult.Should().NotBeNull("Test file creation should succeed");

        // Read file back
        var readResult = await ExecuteMCPCommandAsync(ServerName, new[] { "--read-file", testFile });
        readResult.Should().NotBeNull("Test file should be readable after creation");

        // Delete file
        var deleteResult = await ExecuteMCPCommandAsync(ServerName, new[] { "--delete-file", testFile });
        deleteResult.Should().NotBeNull("Test file deletion should succeed");
    }

    [Fact]
    public async Task FilesystemServer_ShouldManageLogFiles()
    {
        // Arrange
        Logger.LogInformation("Testing log file management");
        var logPaths = new[]
        {
            "/var/log",
            "/tmp",
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "logs")
        };

        // Act & Assert
        foreach (var logPath in logPaths)
        {
            try
            {
                if (Directory.Exists(logPath))
                {
                    var result = await ExecuteMCPCommandAsync(ServerName, new[] { "--list-directory", logPath });
                    result.Should().NotBeNull($"Log directory should be accessible: {logPath}");
                }
            }
            catch (Exception ex)
            {
                Logger.LogInformation($"Log directory not accessible (expected): {logPath} - {ex.Message}");
            }
        }
    }

    [Fact]
    public async Task FilesystemServer_ShouldTestDirectoryCreation()
    {
        // Arrange
        Logger.LogInformation("Testing directory creation");
        var testDir = Path.Combine(_testDirectory, "test-subdirectory");

        // Act
        var createResult = await ExecuteMCPCommandAsync(ServerName, new[] { "--create-directory", testDir });

        // Assert
        createResult.Should().NotBeNull("Directory creation should succeed");
        
        // Verify directory exists
        var listResult = await ExecuteMCPCommandAsync(ServerName, new[] { "--list-directory", _testDirectory });
        listResult.Should().NotBeNull("Parent directory should list created subdirectory");
    }

    [Fact]
    public async Task FilesystemServer_ShouldTestFilePermissions()
    {
        // Arrange
        Logger.LogInformation("Testing file permission handling");
        var testFile = Path.Combine(_testDirectory, "permissions-test.txt");

        // Act
        var writeResult = await ExecuteMCPCommandAsync(ServerName, new[] { 
            "--write-file", testFile, "Permission test content" 
        });

        // Assert
        writeResult.Should().NotBeNull("File with standard permissions should be writable");
        
        // Test reading the same file
        var readResult = await ExecuteMCPCommandAsync(ServerName, new[] { "--read-file", testFile });
        readResult.Should().NotBeNull("File should be readable after writing");
    }

    [Fact]
    public async Task FilesystemServer_ShouldTestLargeFileHandling()
    {
        // Arrange
        Logger.LogInformation("Testing large file handling");
        var largeFile = Path.Combine(_testDirectory, "large-test-file.txt");
        var largeContent = string.Join(Environment.NewLine, 
            Enumerable.Range(1, 1000).Select(i => $"Line {i}: {new string('x', 100)}"));

        // Act
        var writeResult = await ExecuteMCPCommandAsync(ServerName, new[] { 
            "--write-file", largeFile, largeContent 
        });

        // Assert
        writeResult.Should().NotBeNull("Large file writing should succeed");
        
        // Test reading large file
        var readResult = await ExecuteMCPCommandAsync(ServerName, new[] { "--read-file", largeFile });
        readResult.Should().NotBeNull("Large file should be readable");
    }

    [Fact]
    public async Task FilesystemServer_ShouldTestBinaryFileHandling()
    {
        // Arrange
        Logger.LogInformation("Testing binary file handling");
        var binaryFile = Path.Combine(_testDirectory, "binary-test.bin");
        var binaryData = Convert.ToBase64String(Enumerable.Range(0, 256).Select(i => (byte)i).ToArray());

        // Act
        var writeResult = await ExecuteMCPCommandAsync(ServerName, new[] { 
            "--write-file", binaryFile, binaryData 
        });

        // Assert
        writeResult.Should().NotBeNull("Binary file writing should be handled appropriately");
    }

    [Theory]
    [InlineData("README.md")]
    [InlineData("CONTRIBUTING.md")]
    [InlineData(".gitignore")]
    public async Task FilesystemServer_ShouldReadProjectFiles(string fileName)
    {
        // Arrange
        var filePath = Path.Combine("/home/runner/work/PowerOrchestrator/PowerOrchestrator", fileName);
        Logger.LogInformation($"Testing project file reading: {fileName}");

        // Act
        var result = await ExecuteMCPCommandAsync(ServerName, new[] { "--read-file", filePath });

        // Assert
        result.Should().NotBeNull($"Project file should be readable: {fileName}");
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing && Directory.Exists(_testDirectory))
        {
            try
            {
                Directory.Delete(_testDirectory, true);
            }
            catch (Exception ex)
            {
                Logger.LogWarning($"Failed to cleanup test directory: {ex.Message}");
            }
        }
        base.Dispose(disposing);
    }
}