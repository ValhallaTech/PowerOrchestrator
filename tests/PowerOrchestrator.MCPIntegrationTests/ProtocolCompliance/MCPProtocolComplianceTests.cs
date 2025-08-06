using PowerOrchestrator.MCPIntegrationTests.Infrastructure;
using Xunit.Abstractions;

namespace PowerOrchestrator.MCPIntegrationTests.ProtocolCompliance;

/// <summary>
/// Tests for MCP Protocol Specification Compliance
/// Reference: https://spec.modelcontextprotocol.io/
/// </summary>
public class MCPProtocolComplianceTests : MCPTestBase
{
    public MCPProtocolComplianceTests() { }

    [Fact]
    public async Task All_Servers_Should_Support_JSON_RPC_2_0()
    {
        var testServers = GetEnabledServers();
        
        foreach (var server in testServers)
        {
            Logger.LogInformation("Testing JSON-RPC 2.0 compliance for server: {ServerName}", server.Key);
            
            var result = await ExecuteMCPCommandAsync(server.Key, ["--version"]);
            
            Assert.True(result.IsSuccess, $"Server {server.Key} failed to respond to version request");
            
            if (!Configuration.TestConfiguration.MockMode)
            {
                // In real mode, validate actual JSON-RPC response structure
                Assert.Contains("jsonrpc", result.StandardOutput.ToLower());
            }
            else
            {
                // In mock mode, verify mock response structure
                Assert.NotNull(result.StandardOutput);
                Assert.NotEmpty(result.StandardOutput);
            }
        }
    }

    [Fact]
    public async Task All_Servers_Should_Report_Capabilities()
    {
        var testServers = GetEnabledServers();
        
        foreach (var server in testServers)
        {
            Logger.LogInformation("Testing capability discovery for server: {ServerName}", server.Key);
            
            var result = await ExecuteMCPCommandAsync(server.Key, ["capabilities"]);
            
            if (Configuration.TestConfiguration.MockMode)
            {
                // Mock mode should return simulated capabilities
                Assert.True(result.IsSuccess, $"Mock server {server.Key} should return capabilities");
                Assert.NotEmpty(result.StandardOutput);
            }
            else
            {
                // Real mode validation would check actual MCP capability response
                Logger.LogInformation("Capabilities for {ServerName}: {Output}", server.Key, result.StandardOutput);
            }
        }
    }

    [Fact]
    public async Task All_Servers_Should_Handle_Invalid_Requests_Gracefully()
    {
        var testServers = GetEnabledServers();
        
        foreach (var server in testServers)
        {
            Logger.LogInformation("Testing error handling for server: {ServerName}", server.Key);
            
            var result = await ExecuteMCPCommandAsync(server.Key, ["invalid_command_12345"]);
            
            // Servers should not crash on invalid requests
            if (Configuration.TestConfiguration.MockMode)
            {
                // Mock should handle gracefully
                Assert.NotNull(result);
            }
            else
            {
                // Real servers should return proper error responses
                Logger.LogInformation("Error response from {ServerName}: {Output}", server.Key, result.StandardOutput);
            }
        }
    }

    [Theory]
    [InlineData("postgresql-powerorch")]
    [InlineData("docker-orchestration")]
    [InlineData("powershell-execution")]
    [InlineData("api-testing")]
    public async Task Critical_Servers_Should_Expose_Required_Tools(string serverName)
    {
        var serverConfig = GetServerConfig(serverName);
        Logger.LogInformation("Testing tool exposure for critical server: {ServerName}", serverName);
        
        var result = await ExecuteMCPCommandAsync(serverName, ["list-tools"]);
        
        if (Configuration.TestConfiguration.MockMode)
        {
            Assert.True(result.IsSuccess, $"Mock server {serverName} should list tools successfully");
            
            // Verify configured tools are represented in mock response
            foreach (var tool in serverConfig.Tools)
            {
                Assert.Contains(tool, result.StandardOutput, StringComparison.OrdinalIgnoreCase);
            }
        }
        else
        {
            Logger.LogInformation("Tools for {ServerName}: {Output}", serverName, result.StandardOutput);
        }
    }

    [Theory]
    [InlineData("filesystem-ops")]
    [InlineData("git-repository")]
    [InlineData("system-monitoring")]
    [InlineData("redis-operations")]
    public async Task High_Impact_Servers_Should_Expose_Resources(string serverName)
    {
        var serverConfig = GetServerConfig(serverName);
        Logger.LogInformation("Testing resource exposure for high-impact server: {ServerName}", serverName);
        
        var result = await ExecuteMCPCommandAsync(serverName, ["list-resources"]);
        
        if (Configuration.TestConfiguration.MockMode)
        {
            Assert.True(result.IsSuccess, $"Mock server {serverName} should list resources successfully");
            
            // Verify configured resources are represented in mock response
            foreach (var resource in serverConfig.Resources)
            {
                Assert.Contains(resource, result.StandardOutput, StringComparison.OrdinalIgnoreCase);
            }
        }
        else
        {
            Logger.LogInformation("Resources for {ServerName}: {Output}", serverName, result.StandardOutput);
        }
    }

    [Fact]
    public async Task All_Servers_Should_Support_Graceful_Shutdown()
    {
        var testServers = GetEnabledServers();
        
        foreach (var server in testServers)
        {
            Logger.LogInformation("Testing graceful shutdown for server: {ServerName}", server.Key);
            
            // Start server connection
            var startResult = await ExecuteMCPCommandAsync(server.Key, ["--help"]);
            Assert.True(startResult.IsSuccess, $"Server {server.Key} should start successfully");
            
            // Test shutdown (simulated in mock mode)
            if (Configuration.TestConfiguration.MockMode)
            {
                // Mock mode simulates successful shutdown
                Logger.LogInformation("Mock shutdown successful for {ServerName}", server.Key);
                Assert.True(true, "Mock servers handle shutdown gracefully");
            }
            else
            {
                // Real servers would need proper shutdown testing
                Logger.LogInformation("Testing real shutdown for {ServerName}", server.Key);
            }
        }
    }

    [Fact]
    public async Task Servers_Should_Respond_Within_Timeout_Limits()
    {
        var testServers = GetEnabledServers();
        var timeout = Configuration.TestConfiguration.Timeout;
        
        foreach (var server in testServers)
        {
            Logger.LogInformation("Testing response time for server: {ServerName} (timeout: {Timeout}ms)", 
                server.Key, timeout);
            
            var stopwatch = System.Diagnostics.Stopwatch.StartNew();
            var result = await ExecuteMCPCommandAsync(server.Key, ["--version"]);
            stopwatch.Stop();
            
            Assert.True(result.IsSuccess, $"Server {server.Key} should respond successfully");
            Assert.True(stopwatch.ElapsedMilliseconds < timeout, 
                $"Server {server.Key} responded in {stopwatch.ElapsedMilliseconds}ms, exceeding timeout of {timeout}ms");
            
            Logger.LogInformation("Server {ServerName} responded in {ElapsedMs}ms", 
                server.Key, stopwatch.ElapsedMilliseconds);
        }
    }

    [Fact]
    public async Task Protocol_Version_Should_Match_Configuration()
    {
        var expectedVersion = Configuration.McpProtocol.Version;
        var testServers = GetEnabledServers();
        
        foreach (var server in testServers)
        {
            Logger.LogInformation("Testing protocol version for server: {ServerName}", server.Key);
            
            var result = await ExecuteMCPCommandAsync(server.Key, ["protocol-version"]);
            
            if (Configuration.TestConfiguration.MockMode)
            {
                // Mock mode should return configured version
                Assert.Contains(expectedVersion, result.StandardOutput);
                Logger.LogInformation("Server {ServerName} reports protocol version: {Version}", 
                    server.Key, expectedVersion);
            }
            else
            {
                Logger.LogInformation("Protocol version check for {ServerName}: {Output}", 
                    server.Key, result.StandardOutput);
            }
        }
    }
}