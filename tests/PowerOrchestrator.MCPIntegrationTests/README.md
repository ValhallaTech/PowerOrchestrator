# PowerOrchestrator MCP Integration Tests

This test project provides comprehensive testing and validation for PowerOrchestrator's MCP (Model Context Protocol) servers integration. It ensures enterprise-grade development workflow support across all critical infrastructure components.

## Overview

The MCP Integration Tests validate 8 critical MCP servers that form the backbone of PowerOrchestrator's development and operational infrastructure:

### Critical Tier Servers
- **PostgreSQL PowerOrch Server** - Database operations, schema validation, performance testing
- **Docker Orchestration Server** - Container ecosystem management (PostgreSQL + Redis + Seq)
- **PowerShell Execution Server** - Core business logic validation and script security
- **API Testing Server** - API architecture validation and MAUI-to-API communication

### High Impact Tier Servers  
- **Filesystem Operations Server** - Project files and logs management
- **Git Repository Server** - Repository operations and development phase tracking
- **System Monitoring Server** - Performance metrics collection
- **Redis Operations Server** - Cache operations and session management

## Test Structure

```
PowerOrchestrator.MCPIntegrationTests/
├── Configuration/
│   └── mcp-servers.json           # MCP server configurations
├── Infrastructure/
│   ├── MCPTestBase.cs             # Base test infrastructure
│   └── MCPServerConfiguration.cs   # Configuration models
├── CriticalTier/
│   ├── PostgreSQLPowerOrchServerTests.cs
│   ├── DockerOrchestrationServerTests.cs
│   ├── PowerShellExecutionServerTests.cs
│   └── ApiTestingServerTests.cs
├── HighImpactTier/
│   ├── RedisOperationsServerTests.cs
│   ├── FilesystemOpsServerTests.cs
│   ├── GitRepositoryServerTests.cs
│   └── SystemMonitoringServerTests.cs
├── EndToEndWorkflows/
│   └── EndToEndWorkflowTests.cs    # Complete workflow validation
├── PerformanceBenchmarks/
│   └── MCPServerPerformanceBenchmarks.cs
└── TestData/
    └── Scripts/                   # Test scripts and data
```

## Running the Tests

### Prerequisites

1. **Docker Environment**: Ensure Docker and Docker Compose are running
2. **Development Services**: Start the development environment:
   ```bash
   docker-compose -f docker-compose.dev.yml up -d
   ```

3. **Node.js and NPM**: Required for MCP server packages:
   ```bash
   npm install -g @modelcontextprotocol/server-postgres
   npm install -g @modelcontextprotocol/server-docker
   npm install -g @modelcontextprotocol/server-shell
   npm install -g @modelcontextprotocol/server-fetch
   # ... other MCP servers as needed
   ```

### Test Execution

```bash
# Run all MCP integration tests
dotnet test tests/PowerOrchestrator.MCPIntegrationTests

# Run specific test categories
dotnet test tests/PowerOrchestrator.MCPIntegrationTests --filter "Category=CriticalTier"
dotnet test tests/PowerOrchestrator.MCPIntegrationTests --filter "Category=EndToEnd"
dotnet test tests/PowerOrchestrator.MCPIntegrationTests --filter "Category=Performance"

# Run with verbose logging
dotnet test tests/PowerOrchestrator.MCPIntegrationTests --logger "console;verbosity=detailed"
```

### Performance Benchmarks

```bash
# Run performance benchmarks
dotnet run --project tests/PowerOrchestrator.MCPIntegrationTests -c Release

# Run specific benchmarks
dotnet run --project tests/PowerOrchestrator.MCPIntegrationTests -c Release -- --filter "*PostgreSQL*"
```

## Configuration

The MCP servers are configured in `Configuration/mcp-servers.json`:

```json
{
  "mcpServers": {
    "postgresql-powerorch": {
      "type": "local",
      "command": "npx",
      "args": ["@modelcontextprotocol/server-postgres", "postgresql://..."],
      "tools": ["query", "schema", "list_tables", "describe_table", "execute"],
      "priority": "critical"
    }
    // ... other servers
  }
}
```

### Environment Variables

Set these environment variables for testing:

```bash
# Database connection
export ConnectionStrings__DefaultConnection="Host=localhost;Database=powerorchestrator_dev;Username=powerorch;Password=PowerOrch2025!"

# Redis connection  
export Redis__ConnectionString="localhost:6379"
export Redis__Password="PowerOrchRedis2025!"

# API configuration
export Api__BaseUrl="https://localhost:7001"

# Enable/disable test modes
export TestConfiguration__MockMode="false"
export TestConfiguration__PerformanceBenchmarks__Enabled="true"
```

## Test Categories

### 1. Critical Tier Tests

**PostgreSQL PowerOrch Server**
- Database connectivity and schema validation
- Performance testing with enterprise-scale data
- Audit log integrity verification
- Materialized view performance monitoring

**Docker Orchestration Server**
- Container ecosystem management
- Service health checks and dependency validation
- Resource constraints and volume management
- Network configuration validation

**PowerShell Execution Server**
- Sample script execution (hello-world.ps1, system-info.ps1)
- Security validation and error handling
- Module loading and parameter passing
- Performance monitoring and concurrent execution

**API Testing Server**
- Health check and Swagger documentation validation
- Endpoint testing (scripts, executions, repositories)
- CORS configuration and rate limiting
- Authentication and webhook endpoints

### 2. End-to-End Workflow Tests

**Complete Orchestration Workflow**
```
Docker MCP → Database MCP → PowerShell MCP → API MCP → Database MCP
```

**Cross-Phase Integration**
- Phase 2→4: GitHub script discovery → PowerShell execution
- Phase 4→5: Script execution → comprehensive logging
- Phase 5→6: Monitoring → production deployment readiness

**Enterprise Scale Validation**
- Concurrent operations across multiple MCP servers
- Data flow validation through complete pipeline
- Resource usage monitoring under load

### 3. Performance Benchmarks

**Enterprise Performance Requirements**
- Database operations: < 5 seconds
- Cache operations: < 1 second  
- Script operations: < 10 seconds
- API operations: < 2 seconds

**Resource Constraints**
- Memory usage: < 512MB during bulk operations
- Concurrent operations: Support 20+ simultaneous requests
- End-to-end workflow: < 120 seconds completion time

## Acceptance Criteria Validation

The tests validate all acceptance criteria from the original issue:

- ✅ All 8 MCP servers can be successfully initialized and connected
- ✅ PostgreSQL server can execute queries and validate schema integrity
- ✅ Docker server can manage PowerOrchestrator container ecosystem
- ✅ PowerShell server can execute sample scripts from `/scripts/sample-scripts/`
- ✅ API server can test health check endpoints and Swagger documentation
- ✅ Integration tests demonstrate end-to-end workflow capabilities
- ✅ Performance benchmarks establish baseline metrics for enterprise scaling

## Troubleshooting

### Common Issues

**MCP Server Not Found**
```bash
# Install missing MCP server packages
npm install -g @modelcontextprotocol/server-postgres
```

**Docker Services Not Running**
```bash
# Start development environment
docker-compose -f docker-compose.dev.yml up -d

# Check service health
docker-compose -f docker-compose.dev.yml ps
```

**Database Connection Failures**
```bash
# Verify PostgreSQL is accessible
docker exec -it powerorchestrator-postgres-1 psql -U powerorch -d powerorchestrator_dev -c "SELECT version();"
```

**Permission Issues**
```bash
# Ensure PowerShell execution policy allows script execution
Set-ExecutionPolicy -ExecutionPolicy RemoteSigned -Scope CurrentUser
```

### Test Environment Reset

```bash
# Reset test environment
docker-compose -f docker-compose.dev.yml down -v
docker-compose -f docker-compose.dev.yml up -d

# Clear test data
dotnet test tests/PowerOrchestrator.MCPIntegrationTests --filter "Category=Cleanup"
```

## Continuous Integration

The MCP integration tests are designed to integrate with the existing CI/CD pipeline:

```yaml
- name: Run MCP Integration Tests
  run: |
    docker-compose -f docker-compose.dev.yml up -d
    dotnet test tests/PowerOrchestrator.MCPIntegrationTests --logger trx
    docker-compose -f docker-compose.dev.yml down
```

## Contributing

When adding new MCP servers or tests:

1. Add server configuration to `mcp-servers.json`
2. Create test class following existing patterns
3. Add performance benchmarks for critical operations
4. Update this README with new test descriptions
5. Ensure all tests pass in clean environment

## Enterprise Deployment Readiness

These tests validate PowerOrchestrator's readiness for enterprise deployment by ensuring:

- **Scalability**: Performance benchmarks validate enterprise-scale data handling
- **Reliability**: End-to-end workflows validate complete system integration
- **Security**: PowerShell execution security and API authentication validation
- **Monitoring**: Comprehensive logging and audit trail verification
- **Maintainability**: Clear test structure and documentation for ongoing development