namespace PowerOrchestrator.MCPIntegrationTests.Infrastructure;

/// <summary>
/// Configuration model for MCP server definitions matching mcp-servers2.json structure
/// </summary>
public class MCPServerConfiguration
{
    public Dictionary<string, MCPServer> McpServers { get; set; } = new();
}

/// <summary>
/// Individual MCP server configuration matching the actual JSON structure
/// </summary>
public class MCPServer
{
    public string Type { get; set; } = string.Empty;
    public string Command { get; set; } = string.Empty;
    public List<string> Args { get; set; } = new();
    public List<string> Tools { get; set; } = new();
    public List<string>? Resources { get; set; }
}