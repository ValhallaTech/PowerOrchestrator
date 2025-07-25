# PowerOrchestrator 🚀

Enterprise-grade PowerShell script orchestration platform with secure execution, GitHub integration, and comprehensive monitoring.

![.NET 8](https://img.shields.io/badge/.NET-8-purple) 
![PostgreSQL](https://img.shields.io/badge/PostgreSQL-17.5-blue) 
![Redis](https://img.shields.io/badge/Redis-8.0.3-red) 
![License](https://img.shields.io/badge/license-MIT-green)

## 🌟 Features

- **🔐 Secure PowerShell Execution** - Enterprise-grade script execution with comprehensive monitoring
- **📦 GitHub Integration** - Automatic script discovery and version management
- **🎨 Modern UI** - Material Design MAUI application with UraniumUI 2.12.1
- **🛡️ Enterprise Security** - Multi-factor authentication and role-based access control
- **📊 Comprehensive Monitoring** - Real-time execution tracking with structured logging
- **⚡ High Performance** - PostgreSQL 17.5 and Redis 8.0.3 for optimal performance

## 🏗️ Architecture

PowerOrchestrator follows clean architecture principles with clear separation of concerns:

```
src/
├── PowerOrchestrator.MAUI/          # MAUI UI with Material Design
├── PowerOrchestrator.API/           # ASP.NET Core Web API
├── PowerOrchestrator.Application/   # Application layer (CQRS, MediatR)
├── PowerOrchestrator.Domain/        # Domain entities and business logic
├── PowerOrchestrator.Infrastructure/ # Data access and external services
└── PowerOrchestrator.Identity/      # Authentication and authorization
```

## 🚀 Quick Start

### Prerequisites

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- [Docker](https://www.docker.com/) and Docker Compose
- [PostgreSQL 17.5](https://www.postgresql.org/) (or use Docker)
- [Redis 8.0.3](https://redis.io/) (or use Docker)

### Development Setup

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

### Environment URLs

- **API**: http://localhost:5000
- **Swagger UI**: http://localhost:5000/swagger
- **Seq Logs**: http://localhost:5341
- **PostgreSQL**: localhost:5432
- **Redis**: localhost:6379

## 🛠️ Technology Stack

### Core Framework
- **.NET 8** with C# 12 language features
- **ASP.NET Core 8** for Web API
- **MAUI** for cross-platform UI

### UI & Design
- **UraniumUI 2.12.1** for Material Design components
- **Material Icons** for consistent iconography
- **Responsive design** for desktop and mobile

### Data & Caching
- **PostgreSQL 17.5** with advanced features and optimization
- **Entity Framework Core 8** for data access
- **Dapper 2.1.66** for high-performance queries
- **Redis 8.0.3** for caching and session management

### Architecture & Patterns
- **Clean Architecture** with clear layer separation
- **CQRS** with MediatR for command/query separation
- **Repository Pattern** for data access abstraction
- **Dependency Injection** with Autofac 8.3.0

### Monitoring & Logging
- **Serilog 4.3.0** for structured logging
- **Seq** for log aggregation and analysis
- **Health checks** for service monitoring
- **Performance counters** for metrics

### Security
- **JWT Bearer Authentication** for API security
- **Identity Framework** for user management
- **Role-based authorization** for access control
- **Data protection** for sensitive information

### Integration
- **Octokit 14.0.0** for GitHub API integration
- **PowerShell SDK 7.4.6** for script execution
- **AutoMapper** for object mapping
- **FluentValidation** for business rule validation

## 📊 Development Environment

The development environment uses Docker Compose for easy setup:

### Services

| Service | Version | Port | Purpose |
|---------|---------|------|---------|
| PostgreSQL | 17.5 | 5432 | Primary database |
| Redis | 8.0.3 | 6379 | Caching & sessions |
| Seq | Latest | 5341 | Log aggregation |

### Database Configuration

- **Database**: `powerorchestrator_dev`
- **User**: `powerorch`
- **Password**: `PowerOrch2025!`
- **Features**: UUID, crypto extensions, performance optimization

### Redis Configuration

- **Password**: `PowerOrchRedis2025!`
- **Persistence**: AOF enabled
- **Memory**: 512MB with LRU eviction

## 🧪 Testing

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

- **Unit Tests**: Domain logic and business rules
- **Integration Tests**: API endpoints and database operations
- **Load Tests**: Performance and scalability validation

## 🔧 Configuration

### Application Settings

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

### Environment Variables

| Variable | Description | Default |
|----------|-------------|---------|
| `ASPNETCORE_ENVIRONMENT` | Runtime environment | `Development` |
| `ConnectionStrings__DefaultConnection` | Database connection | See above |
| `ConnectionStrings__Redis` | Redis connection | `localhost:6379` |

## 📚 Documentation

Comprehensive documentation is available in the `docs/` directory:

- **[Architecture Guide](docs/architecture/)** - System design and patterns
- **[User Guide](docs/user-guide/)** - End-user documentation
- **[Developer Guide](docs/developer-guide/)** - Development guidelines
- **[API Documentation](docs/api/)** - REST API reference

## 🤝 Contributing

We welcome contributions! Please see [CONTRIBUTING.md](CONTRIBUTING.md) for guidelines.

### Development Workflow

1. Fork the repository
2. Create a feature branch
3. Make your changes
4. Run tests and ensure they pass
5. Submit a pull request

### Code Quality

- Follow C# coding conventions
- Write unit tests for new features
- Update documentation as needed
- Ensure all CI checks pass

## 📄 License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## 🌐 Support

- **Issues**: [GitHub Issues](https://github.com/ValhallaTech/PowerOrchestrator/issues)
- **Discussions**: [GitHub Discussions](https://github.com/ValhallaTech/PowerOrchestrator/discussions)
- **Wiki**: [Project Wiki](https://github.com/ValhallaTech/PowerOrchestrator/wiki)

## 🔮 Roadmap

### Phase 1 (Aug 1-15): Core Infrastructure & Database Design
- Complete database schema design
- Implement core domain entities
- Set up data access layer

### Phase 2 (Aug 15-29): GitHub Integration & Repository Management
- GitHub API integration
- Repository scanning and script discovery
- Version management system

### Phase 2.5 (Aug 29-Sep 19): Identity Management & Security
- User authentication and authorization
- Role-based access control
- Security audit logging

### Phase 3 (Sep 19-Oct 10): MAUI Application & Material Design UI
- Complete MAUI application
- Material Design implementation
- Cross-platform deployment

---

**Built with ❤️ by the ValhallaTech team**
