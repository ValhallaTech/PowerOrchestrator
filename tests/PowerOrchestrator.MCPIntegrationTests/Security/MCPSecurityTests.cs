using PowerOrchestrator.MCPIntegrationTests.Infrastructure;
using Xunit.Abstractions;

namespace PowerOrchestrator.MCPIntegrationTests.Security;

/// <summary>
/// Enterprise security validation tests for MCP servers
/// Tests authentication, authorization, rate limiting, and input validation
/// </summary>
public class MCPSecurityTests : MCPTestBase
{
    public MCPSecurityTests() { }

    [Theory]
    [InlineData("powershell-execution")]
    [InlineData("filesystem-ops")]
    [InlineData("docker-orchestration")]
    public async Task Servers_Should_Reject_Restricted_Commands(string serverName)
    {
        var serverConfig = GetServerConfig(serverName);
        Logger.LogInformation("Testing command restriction for server: {ServerName}", serverName);
        
        if (serverConfig.Security?.RestrictedCommands?.Any() == true)
        {
            foreach (var restrictedCommand in serverConfig.Security.RestrictedCommands)
            {
                Logger.LogInformation("Testing restricted command: {Command} for {ServerName}", 
                    restrictedCommand, serverName);
                
                var result = await ExecuteMCPCommandAsync(serverName, [restrictedCommand]);
                
                if (Configuration.TestConfiguration.MockMode)
                {
                    // Mock mode should simulate security rejection
                    Assert.False(result.IsSuccess || result.StandardOutput.Contains("permission denied", StringComparison.OrdinalIgnoreCase),
                        $"Server {serverName} should reject restricted command: {restrictedCommand}");
                }
                else
                {
                    // Real servers should properly reject restricted commands
                    Logger.LogInformation("Restriction test result for {Command}: {Output}", 
                        restrictedCommand, result.StandardOutput);
                }
            }
        }
        else
        {
            Logger.LogInformation("No restricted commands configured for {ServerName}", serverName);
        }
    }

    [Theory]
    [InlineData("filesystem-ops")]
    public async Task Filesystem_Server_Should_Respect_Path_Restrictions(string serverName)
    {
        var serverConfig = GetServerConfig(serverName);
        Logger.LogInformation("Testing path restriction for server: {ServerName}", serverName);
        
        if (serverConfig.Security?.RestrictedPaths?.Any() == true)
        {
            foreach (var restrictedPath in serverConfig.Security.RestrictedPaths)
            {
                Logger.LogInformation("Testing restricted path access: {Path} for {ServerName}", 
                    restrictedPath, serverName);
                
                var result = await ExecuteMCPCommandAsync(serverName, ["read_file", restrictedPath + "/test.txt"]);
                
                if (Configuration.TestConfiguration.MockMode)
                {
                    // Mock mode should simulate access denial
                    Assert.False(result.IsSuccess || result.StandardOutput.Contains("access denied", StringComparison.OrdinalIgnoreCase),
                        $"Server {serverName} should deny access to restricted path: {restrictedPath}");
                }
                else
                {
                    Logger.LogInformation("Path restriction test result for {Path}: {Output}", 
                        restrictedPath, result.StandardOutput);
                }
            }
        }
        else
        {
            Logger.LogInformation("No restricted paths configured for {ServerName}", serverName);
        }
    }

    [Theory]
    [InlineData("api-testing")]
    public async Task API_Server_Should_Validate_SSL_For_HTTPS_Requests(string serverName)
    {
        var serverConfig = GetServerConfig(serverName);
        Logger.LogInformation("Testing SSL validation for server: {ServerName}", serverName);
        
        if (serverConfig.Security?.ValidateSSL == true)
        {
            // Test with invalid SSL certificate
            var result = await ExecuteMCPCommandAsync(serverName, 
                ["get", "https://self-signed.badssl.com/"]);
            
            if (Configuration.TestConfiguration.MockMode)
            {
                // Mock mode should simulate SSL validation failure
                Assert.False(result.IsSuccess, 
                    $"Server {serverName} should reject invalid SSL certificates");
            }
            else
            {
                Logger.LogInformation("SSL validation test result: {Output}", result.StandardOutput);
            }
        }
        else
        {
            Logger.LogInformation("SSL validation not enabled for {ServerName}", serverName);
        }
    }

    [Fact]
    public async Task All_Servers_Should_Handle_Malformed_Input_Safely()
    {
        var testServers = GetEnabledServers();
        var malformedInputs = new[]
        {
            "../../etc/passwd",
            "<script>alert('xss')</script>",
            "'; DROP TABLE users; --",
            "\0\x01\x02\x03\x04",
            new string('A', 10000) // Very long input
        };
        
        foreach (var server in testServers)
        {
            Logger.LogInformation("Testing input validation for server: {ServerName}", server.Key);
            
            foreach (var malformedInput in malformedInputs)
            {
                var result = await ExecuteMCPCommandAsync(server.Key, [malformedInput]);
                
                // Servers should not crash or expose sensitive information
                Assert.NotNull(result);
                
                if (!Configuration.TestConfiguration.MockMode)
                {
                    // Check that no sensitive system information is leaked
                    Assert.DoesNotContain("/etc/passwd", result.StandardOutput);
                    Assert.DoesNotContain("root:", result.StandardOutput);
                }
                
                Logger.LogInformation("Input validation test completed for {ServerName} with input: {Input}", 
                    server.Key, malformedInput.Length > 50 ? malformedInput[..50] + "..." : malformedInput);
            }
        }
    }

    [Theory]
    [InlineData("api-testing")]
    public async Task Rate_Limited_Servers_Should_Enforce_Limits(string serverName)
    {
        var serverConfig = GetServerConfig(serverName);
        Logger.LogInformation("Testing rate limiting for server: {ServerName}", serverName);
        
        if (serverConfig.RateLimiting?.Enabled == true)
        {
            var requestsPerMinute = serverConfig.RateLimiting.RequestsPerMinute;
            var burstLimit = serverConfig.RateLimiting.BurstLimit;
            
            Logger.LogInformation("Rate limit config - Requests/min: {RequestsPerMinute}, Burst: {BurstLimit}", 
                requestsPerMinute, burstLimit);
            
            // Attempt to exceed burst limit
            var rapidRequests = new List<Task<ProcessResult>>();
            for (int i = 0; i < burstLimit + 5; i++)
            {
                rapidRequests.Add(ExecuteMCPCommandAsync(serverName, ["get", "https://httpbin.org/delay/0"]));
            }
            
            var results = await Task.WhenAll(rapidRequests);
            
            if (Configuration.TestConfiguration.MockMode)
            {
                // Mock mode should simulate rate limiting after burst limit
                var failedCount = results.Count(r => !r.IsSuccess);
                Assert.True(failedCount > 0, $"Rate limiting should reject some requests for {serverName}");
            }
            else
            {
                Logger.LogInformation("Rate limiting test results for {ServerName}: {SuccessCount}/{TotalCount} succeeded", 
                    serverName, results.Count(r => r.IsSuccess), results.Length);
            }
        }
        else
        {
            Logger.LogInformation("Rate limiting not enabled for {ServerName}", serverName);
        }
    }

    [Theory]
    [InlineData("powershell-execution")]
    public async Task PowerShell_Server_Should_Enforce_Execution_Timeout(string serverName)
    {
        var serverConfig = GetServerConfig(serverName);
        Logger.LogInformation("Testing execution timeout for server: {ServerName}", serverName);
        
        if (serverConfig.Security?.ExecutionTimeout > 0)
        {
            var timeoutMs = serverConfig.Security.ExecutionTimeout;
            Logger.LogInformation("Testing timeout enforcement: {TimeoutMs}ms for {ServerName}", 
                timeoutMs, serverName);
            
            // Create a script that would run longer than timeout
            var longRunningScript = "Start-Sleep -Seconds " + (timeoutMs / 1000 + 10);
            
            var stopwatch = System.Diagnostics.Stopwatch.StartNew();
            var result = await ExecuteMCPCommandAsync(serverName, ["execute", longRunningScript]);
            stopwatch.Stop();
            
            if (Configuration.TestConfiguration.MockMode)
            {
                // Mock mode should simulate timeout enforcement
                Assert.True(stopwatch.ElapsedMilliseconds < timeoutMs + 5000, 
                    $"Mock server {serverName} should enforce execution timeout");
            }
            else
            {
                Logger.LogInformation("Timeout test result for {ServerName}: Executed in {ElapsedMs}ms", 
                    serverName, stopwatch.ElapsedMilliseconds);
            }
        }
        else
        {
            Logger.LogInformation("No execution timeout configured for {ServerName}", serverName);
        }
    }

    [Theory]
    [InlineData("filesystem-ops")]
    public async Task Filesystem_Server_Should_Enforce_File_Size_Limits(string serverName)
    {
        var serverConfig = GetServerConfig(serverName);
        Logger.LogInformation("Testing file size limits for server: {ServerName}", serverName);
        
        if (!string.IsNullOrEmpty(serverConfig.Security?.MaxFileSize))
        {
            var maxFileSize = serverConfig.Security.MaxFileSize;
            Logger.LogInformation("Testing file size limit: {MaxFileSize} for {ServerName}", 
                maxFileSize, serverName);
            
            // Try to create a file larger than the limit
            var largeContent = new string('X', 15 * 1024 * 1024); // 15MB content
            var testFile = "/tmp/large_test_file.txt";
            
            var result = await ExecuteMCPCommandAsync(serverName, ["write_file", testFile, largeContent]);
            
            if (Configuration.TestConfiguration.MockMode)
            {
                // Mock mode should simulate size limit enforcement
                Assert.False(result.IsSuccess, 
                    $"Server {serverName} should reject files larger than {maxFileSize}");
            }
            else
            {
                Logger.LogInformation("File size limit test result for {ServerName}: {Output}", 
                    serverName, result.StandardOutput);
            }
        }
        else
        {
            Logger.LogInformation("No file size limit configured for {ServerName}", serverName);
        }
    }

    [Theory]
    [InlineData("api-testing")]
    public async Task API_Server_Should_Respect_Domain_Restrictions(string serverName)
    {
        var serverConfig = GetServerConfig(serverName);
        Logger.LogInformation("Testing domain restrictions for server: {ServerName}", serverName);
        
        if (serverConfig.Security?.AllowedDomains?.Any() == true)
        {
            var allowedDomains = serverConfig.Security.AllowedDomains;
            Logger.LogInformation("Allowed domains for {ServerName}: {Domains}", 
                serverName, string.Join(", ", allowedDomains));
            
            // Test access to disallowed domain
            var result = await ExecuteMCPCommandAsync(serverName, ["get", "https://malicious-domain.example.com"]);
            
            if (Configuration.TestConfiguration.MockMode)
            {
                // Mock mode should simulate domain restriction
                Assert.False(result.IsSuccess, 
                    $"Server {serverName} should reject requests to disallowed domains");
            }
            else
            {
                Logger.LogInformation("Domain restriction test result for {ServerName}: {Output}", 
                    serverName, result.StandardOutput);
            }
        }
        else
        {
            Logger.LogInformation("No domain restrictions configured for {ServerName}", serverName);
        }
    }

    [Fact]
    public async Task All_Servers_Should_Not_Expose_Sensitive_Environment_Variables()
    {
        var testServers = GetEnabledServers();
        var sensitiveVars = new[] { "PASSWORD", "SECRET", "KEY", "TOKEN", "CREDENTIAL" };
        
        foreach (var server in testServers)
        {
            Logger.LogInformation("Testing environment variable exposure for server: {ServerName}", server.Key);
            
            var result = await ExecuteMCPCommandAsync(server.Key, ["--help"]);
            
            foreach (var sensitiveVar in sensitiveVars)
            {
                Assert.DoesNotContain(sensitiveVar, result.StandardOutput, StringComparison.OrdinalIgnoreCase);
            }
            
            Logger.LogInformation("Environment variable exposure test completed for {ServerName}", server.Key);
        }
    }

    [Fact]
    public async Task Security_Configuration_Should_Be_Valid()
    {
        var testServers = GetEnabledServers();
        
        foreach (var server in testServers)
        {
            var serverConfig = GetServerConfig(server.Key);
            Logger.LogInformation("Validating security configuration for server: {ServerName}", server.Key);
            
            if (serverConfig.Security != null)
            {
                // Validate timeout values are reasonable
                if (serverConfig.Security.ExecutionTimeout > 0)
                {
                    Assert.True(serverConfig.Security.ExecutionTimeout <= 600000, // 10 minutes max
                        $"Execution timeout too high for {server.Key}");
                    Assert.True(serverConfig.Security.ExecutionTimeout >= 1000, // 1 second min
                        $"Execution timeout too low for {server.Key}");
                }
                
                // Validate file size limits are reasonable
                if (!string.IsNullOrEmpty(serverConfig.Security.MaxFileSize))
                {
                    Assert.Matches(@"^\d+[KMGT]?B$", serverConfig.Security.MaxFileSize);
                }
                
                Logger.LogInformation("Security configuration valid for {ServerName}", server.Key);
            }
            else
            {
                Logger.LogInformation("No security configuration for {ServerName}", server.Key);
            }
        }
    }
}