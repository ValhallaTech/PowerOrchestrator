# Development Environment Setup

Welcome to the PowerOrchestrator development team! This guide will help you set up your development environment and get started with contributing to the project.

## Prerequisites

Before you begin, ensure you have the following installed on your development machine:

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- [Docker](https://www.docker.com/) and Docker Compose
- [PostgreSQL 17.5](https://www.postgresql.org/) (or use Docker)
- [Redis 8.0.3](https://redis.io/) (or use Docker)

## Quick Setup

### 1. Clone the Repository

```bash
git clone https://github.com/ValhallaTech/PowerOrchestrator.git
cd PowerOrchestrator
```

### 2. Start Development Environment

The development environment uses Docker Compose for easy setup with PostgreSQL, Redis, and Seq logging:

```bash
# Start PostgreSQL, Redis, and Seq with Docker
docker compose -f docker-compose.dev.yml up -d
```

### 3. Restore Dependencies and Build

```bash
dotnet restore
dotnet build
```

### 4. Run Tests

```bash
dotnet test
```

### 5. Start the API

```bash
cd src/PowerOrchestrator.API
dotnet run
```

### 6. Launch MAUI Application (Optional)

```bash
cd src/PowerOrchestrator.MAUI
dotnet run
```

## Development Services

Once your environment is running, you'll have access to:

| Service | URL | Purpose |
|---------|-----|---------|
| **API** | http://localhost:5000 | REST API endpoints |
| **Swagger UI** | http://localhost:5000/swagger | API documentation and testing |
| **Seq Logs** | http://localhost:5341 | Log aggregation and analysis |
| **PostgreSQL** | localhost:5432 | Primary database |
| **Redis** | localhost:6379 | Caching and session management |

## Database Configuration

The development environment includes a pre-configured PostgreSQL instance:

- **Database**: `powerorchestrator_dev`
- **User**: `powerorch`
- **Password**: `PowerOrch2025!`
- **Features**: UUID extensions, crypto extensions, performance optimization

## Redis Configuration

Redis is configured with:

- **Password**: `PowerOrchRedis2025!`
- **Persistence**: AOF enabled
- **Memory**: 512MB with LRU eviction policy

## Testing

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

### Test Categories

- **Unit Tests**: Domain logic and business rules validation
- **Integration Tests**: API endpoints and database operations
- **Load Tests**: Performance and scalability validation

## Configuration

### Application Settings

Key configuration sections in `appsettings.Development.json`:

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

### Environment Variables

| Variable | Description | Default |
|----------|-------------|---------|
| `ASPNETCORE_ENVIRONMENT` | Runtime environment | `Development` |
| `ConnectionStrings__DefaultConnection` | Database connection | See above |
| `ConnectionStrings__Redis` | Redis connection | `localhost:6379` |

## Troubleshooting

### Common Issues

**Docker services not starting:**
```bash
# Check Docker status
docker ps
docker compose -f docker-compose.dev.yml logs
```

**Build errors:**
```bash
# Clean and restore
dotnet clean
dotnet restore
dotnet build
```

**Database connection issues:**
```bash
# Verify PostgreSQL is running
docker compose -f docker-compose.dev.yml exec postgres psql -U powerorch -d powerorchestrator_dev -c "SELECT version();"
```

## Next Steps

Once your development environment is set up:

1. Review the [Contributing Guidelines](contributing.md)
2. Explore the [Architecture Documentation](../architecture/overview.md)
3. Check out the [API Documentation](../api/overview.md)
4. Browse the codebase and familiarize yourself with the project structure

## Getting Help

- **Issues**: [GitHub Issues](https://github.com/ValhallaTech/PowerOrchestrator/issues)
- **Discussions**: [GitHub Discussions](https://github.com/ValhallaTech/PowerOrchestrator/discussions)
- **Wiki**: [Project Wiki](https://github.com/ValhallaTech/PowerOrchestrator/wiki)

---

**Happy coding! 🚀**