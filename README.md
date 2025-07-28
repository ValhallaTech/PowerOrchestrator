# PowerOrchestrator 🚀

Enterprise-grade PowerShell script orchestration platform with secure execution, GitHub integration, and comprehensive monitoring.

![.NET 8](https://img.shields.io/badge/.NET-8-purple) 
![PostgreSQL](https://img.shields.io/badge/PostgreSQL-17.5-blue) 
![Redis](https://img.shields.io/badge/Redis-8.0.3-red)

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

### Get Started

```bash
# Clone and setup
git clone https://github.com/ValhallaTech/PowerOrchestrator.git
cd PowerOrchestrator

# Start services
docker compose -f docker-compose.dev.yml up -d

# Build and run
dotnet restore && dotnet build
cd src/PowerOrchestrator.API && dotnet run
```

**Access Points:**
- **API**: http://localhost:5000
- **Swagger**: http://localhost:5000/swagger  
- **Logs**: http://localhost:5341

> **💡 For detailed setup instructions, see [Developer Setup Guide](docs/developer-guide/setup.md)**

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

| Service | Version | Port | Purpose |
|---------|---------|------|---------|
| PostgreSQL | 17.5 | 5432 | Primary database |
| Redis | 8.0.3 | 6379 | Caching & sessions |
| Seq | Latest | 5341 | Log aggregation |

## 🧪 Testing

PowerOrchestrator includes comprehensive testing at multiple levels:

```bash
# All tests
dotnet test

# Specific test projects
dotnet test tests/PowerOrchestrator.UnitTests
dotnet test tests/PowerOrchestrator.IntegrationTests
dotnet test tests/PowerOrchestrator.LoadTests
```

## 🔧 Configuration

PowerOrchestrator supports flexible configuration for different environments. Key configuration areas include database connections, Redis caching, logging levels, and security settings.

> **📖 For detailed configuration options, see [Developer Setup Guide](docs/developer-guide/setup.md)**

## 📚 Documentation

Comprehensive documentation is available in the `docs/` directory:

- **[Architecture Guide](docs/architecture/overview.md)** - System design and patterns
- **[User Guide](docs/user-guide/getting-started.md)** - End-user documentation
- **[Developer Guide](docs/developer-guide/setup.md)** - Development setup and guidelines
- **[API Documentation](docs/api/overview.md)** - REST API reference
- **[Phase Development Plan](docs/POrch-PhasePlan.md)** - Project roadmap and implementation phases

## 🤝 Contributing

We welcome contributions! Please see our [Contributing Guidelines](docs/developer-guide/contributing.md) for details on:

- Development workflow and standards  
- Code quality requirements
- Pull request process
- Testing guidelines

## 🌐 Support

- **Issues**: [GitHub Issues](https://github.com/ValhallaTech/PowerOrchestrator/issues)
- **Discussions**: [GitHub Discussions](https://github.com/ValhallaTech/PowerOrchestrator/discussions)
- **Wiki**: [Project Wiki](https://github.com/ValhallaTech/PowerOrchestrator/wiki)

## 🔮 Roadmap

PowerOrchestrator is being developed in phases, with each phase building comprehensive functionality:

**Current Phase**: Core Infrastructure & Database Design  
**Next Phase**: GitHub Integration & Repository Management

For detailed phase information and timelines, see the [Phase Development Plan](docs/POrch-PhasePlan.md).

---

**Built with ❤️ by the ValhallaTech team**
