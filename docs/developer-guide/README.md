# PowerOrchestrator Developer Guide

This directory contains development documentation for PowerOrchestrator contributors and developers.

## Development Setup

### Prerequisites

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- [Docker](https://www.docker.com/) and Docker Compose
- [PostgreSQL 17.5](https://www.postgresql.org/) (or use Docker)
- [Redis 8.0.3](https://redis.io/) (or use Docker)

### Getting Started

1. **Clone the repository**
   ```bash
   git clone https://github.com/ValhallaTech/PowerOrchestrator.git
   cd PowerOrchestrator
   ```

2. **Start development environment**
   ```bash
   # Start PostgreSQL, Redis, and Seq with Docker
   docker compose -f docker-compose.dev.yml up -d
   ```

3. **Restore and build**
   ```bash
   dotnet restore
   dotnet build
   ```

4. **Run tests**
   ```bash
   dotnet test
   ```

5. **Start the API**
   ```bash
   cd src/PowerOrchestrator.API
   dotnet run
   ```

6. **Launch MAUI app** (optional)
   ```bash
   cd src/PowerOrchestrator.MAUI
   dotnet run
   ```

### Development Environment

The development environment uses Docker Compose for easy setup:

#### Services

| Service | Version | Port | Purpose |
|---------|---------|------|---------|
| PostgreSQL | 17.5 | 5432 | Primary database |
| Redis | 8.0.3 | 6379 | Caching & sessions |
| Seq | Latest | 5341 | Log aggregation |

#### Environment URLs

- **API**: http://localhost:5000
- **Swagger UI**: http://localhost:5000/swagger
- **Seq Logs**: http://localhost:5341
- **PostgreSQL**: localhost:5432
- **Redis**: localhost:6379

#### Database Configuration

- **Database**: `powerorchestrator_dev`
- **User**: `powerorch`
- **Password**: `PowerOrch2025!`
- **Features**: UUID, crypto extensions, performance optimization

#### Redis Configuration

- **Password**: `PowerOrchRedis2025!`
- **Persistence**: AOF enabled
- **Memory**: 512MB with LRU eviction

### Testing

PowerOrchestrator includes comprehensive testing at multiple levels:

```bash
# Unit tests
dotnet test tests/PowerOrchestrator.UnitTests

# Integration tests (requires Docker services)
dotnet test tests/PowerOrchestrator.IntegrationTests

# Load tests
dotnet test tests/PowerOrchestrator.LoadTests

# All tests with coverage
dotnet test --collect:"XPlat Code Coverage"
```

#### Test Categories

- **Unit Tests**: Domain logic and business rules
- **Integration Tests**: API endpoints and database operations
- **Load Tests**: Performance and scalability validation

### Configuration

#### Application Settings

Key configuration sections:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Database=powerorchestrator_dev;Username=powerorch;Password=PowerOrch2025!",
    "Redis": "localhost:6379"
  },
  "Serilog": {
    "MinimumLevel": "Information",
    "WriteTo": [
      { "Name": "Console" },
      { "Name": "Seq", "Args": { "serverUrl": "http://localhost:5341" } }
    ]
  }
}
```

#### Environment Variables

| Variable | Description | Default |
|----------|-------------|---------|
| `ASPNETCORE_ENVIRONMENT` | Runtime environment | `Development` |
| `ConnectionStrings__DefaultConnection` | Database connection | See above |
| `ConnectionStrings__Redis` | Redis connection | `localhost:6379` |

### Contributing

#### Development Workflow

1. Fork the repository
2. Create a feature branch
3. Make your changes
4. Run tests and ensure they pass
5. Submit a pull request

#### Code Quality

- Follow C# coding conventions
- Write unit tests for new features
- Update documentation as needed
- Ensure all CI checks pass

For detailed contribution guidelines, see [CONTRIBUTING.md](../../CONTRIBUTING.md).

## Related Documents

- [Architecture Guide](../architecture/) - System design and patterns
- [API Documentation](../api/) - REST API reference
- [User Guide](../user-guide/) - End-user documentation