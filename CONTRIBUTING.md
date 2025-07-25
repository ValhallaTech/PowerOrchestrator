# Contributing to PowerOrchestrator

Thank you for your interest in contributing to PowerOrchestrator! This document provides guidelines and information for contributors.

## 🚀 Getting Started

### Prerequisites

Before contributing, ensure you have:

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- [Docker](https://www.docker.com/) and Docker Compose
- [Git](https://git-scm.com/)
- A code editor ([VS Code](https://code.visualstudio.com/), [Visual Studio](https://visualstudio.microsoft.com/), or [Rider](https://www.jetbrains.com/rider/))

### Development Environment Setup

1. **Fork and clone the repository**
   ```bash
   git clone https://github.com/YourUsername/PowerOrchestrator.git
   cd PowerOrchestrator
   ```

2. **Start the development environment**
   ```bash
   docker compose -f docker-compose.dev.yml up -d
   ```

3. **Restore dependencies and build**
   ```bash
   dotnet restore
   dotnet build
   ```

4. **Run tests to ensure everything works**
   ```bash
   dotnet test
   ```

## 🏗️ Project Structure

Understanding the project structure will help you contribute effectively:

```
PowerOrchestrator/
├── src/                           # Source code
│   ├── PowerOrchestrator.MAUI/    # MAUI UI application
│   ├── PowerOrchestrator.API/     # ASP.NET Core Web API
│   ├── PowerOrchestrator.Application/ # Application layer
│   ├── PowerOrchestrator.Domain/  # Domain entities
│   ├── PowerOrchestrator.Infrastructure/ # Data access
│   └── PowerOrchestrator.Identity/ # Authentication
├── tests/                         # Test projects
├── docs/                          # Documentation
├── scripts/                       # Database and sample scripts
└── deployment/                    # Deployment configurations
```

## 🎯 How to Contribute

### Types of Contributions

We welcome various types of contributions:

- **Bug fixes** - Fix existing issues
- **Features** - Add new functionality
- **Documentation** - Improve or add documentation
- **Tests** - Add or improve test coverage
- **Performance** - Optimize code performance
- **Refactoring** - Improve code quality

### Contribution Workflow

1. **Find or create an issue**
   - Check [existing issues](https://github.com/ValhallaTech/PowerOrchestrator/issues)
   - Create a new issue if needed
   - Comment on the issue to indicate you're working on it

2. **Create a feature branch**
   ```bash
   git checkout -b feature/your-feature-name
   # or
   git checkout -b fix/issue-number-description
   ```

3. **Make your changes**
   - Follow the coding standards (see below)
   - Write tests for new functionality
   - Update documentation if needed

4. **Test your changes**
   ```bash
   dotnet build
   dotnet test
   ```

5. **Commit your changes**
   ```bash
   git add .
   git commit -m "feat: add new feature description"
   ```

6. **Push to your fork**
   ```bash
   git push origin feature/your-feature-name
   ```

7. **Create a Pull Request**
   - Use a descriptive title
   - Fill out the PR template
   - Link to related issues

## 📝 Coding Standards

### C# Coding Conventions

Follow Microsoft's [C# Coding Conventions](https://docs.microsoft.com/en-us/dotnet/csharp/programming-guide/inside-a-program/coding-conventions):

- Use PascalCase for public members
- Use camelCase for private members
- Use meaningful and descriptive names
- Keep methods small and focused
- Add XML documentation for public APIs

### Code Organization

- **Domain Layer**: Pure business logic, no external dependencies
- **Application Layer**: Use cases, commands, queries (CQRS pattern)
- **Infrastructure Layer**: Data access, external services, framework concerns
- **API Layer**: Controllers, DTOs, API-specific logic
- **UI Layer**: User interface, view models, UI-specific logic

### Example Code Structure

```csharp
namespace PowerOrchestrator.Domain.Entities
{
    /// <summary>
    /// Represents a PowerShell script in the system
    /// </summary>
    public class Script
    {
        public Guid Id { get; private set; }
        public string Name { get; private set; }
        public string Content { get; private set; }
        
        public Script(string name, string content)
        {
            Id = Guid.NewGuid();
            Name = name ?? throw new ArgumentNullException(nameof(name));
            Content = content ?? throw new ArgumentNullException(nameof(content));
        }
    }
}
```

## 🧪 Testing Guidelines

### Test Structure

- **Unit Tests**: Test individual components in isolation
- **Integration Tests**: Test component interactions
- **Load Tests**: Test performance under load

### Test Naming Convention

```csharp
[Fact]
public void MethodName_StateUnderTest_ExpectedBehavior()
{
    // Arrange
    var expected = "expected result";
    
    // Act
    var actual = MethodUnderTest();
    
    // Assert
    actual.Should().Be(expected);
}
```

### Test Requirements

- All new features must include tests
- Aim for high test coverage (>80%)
- Use meaningful test names
- Follow AAA pattern (Arrange, Act, Assert)
- Use FluentAssertions for readable assertions

## 📚 Documentation

### Code Documentation

- Add XML documentation for public APIs
- Use clear and concise comments
- Explain complex business logic
- Document architectural decisions

### Documentation Updates

When making changes, update relevant documentation:

- API documentation for new endpoints
- README.md for setup changes
- Architecture docs for design changes
- User guides for new features

## 🔀 Git Guidelines

### Commit Messages

Use [Conventional Commits](https://www.conventionalcommits.org/) format:

```
type(scope): description

[optional body]

[optional footer]
```

Types:
- `feat`: New feature
- `fix`: Bug fix
- `docs`: Documentation changes
- `style`: Code style changes (no logic changes)
- `refactor`: Code refactoring
- `test`: Adding or updating tests
- `chore`: Maintenance tasks

Examples:
```
feat(api): add script execution endpoint
fix(database): resolve connection timeout issue
docs(readme): update setup instructions
test(domain): add unit tests for script entity
```

### Branch Naming

- `feature/description` - New features
- `fix/issue-number-description` - Bug fixes
- `docs/description` - Documentation updates
- `refactor/description` - Code refactoring

## 🔍 Code Review Process

### Before Submitting PR

- [ ] Code builds without warnings
- [ ] All tests pass
- [ ] New tests added for new functionality
- [ ] Documentation updated if needed
- [ ] Commits follow conventional format
- [ ] PR description is clear and complete

### Review Criteria

Reviewers will check:

- **Functionality**: Does the code work as intended?
- **Code Quality**: Is the code clean and maintainable?
- **Testing**: Are there adequate tests?
- **Documentation**: Is the documentation updated?
- **Performance**: Are there any performance concerns?
- **Security**: Are there any security implications?

## 🚀 Release Process

### Version Management

We use [Semantic Versioning](https://semver.org/):

- **MAJOR**: Breaking changes
- **MINOR**: New features (backward compatible)
- **PATCH**: Bug fixes (backward compatible)

### Release Cycle

- **Main branch**: Stable releases
- **Develop branch**: Development integration
- **Feature branches**: Individual features
- **Release branches**: Release preparation

## 🆘 Getting Help

If you need help or have questions:

1. **Check existing documentation**
2. **Search existing issues**
3. **Ask in GitHub Discussions**
4. **Create a new issue** with the `question` label

## 🎉 Recognition

Contributors will be recognized in:

- **CONTRIBUTORS.md** file
- **Release notes** for significant contributions
- **GitHub contributors** section

## 📋 Issue Labels

We use labels to organize issues:

- **Type**: `bug`, `feature`, `documentation`, `question`
- **Priority**: `low`, `medium`, `high`, `critical`
- **Difficulty**: `good-first-issue`, `help-wanted`, `expert-level`
- **Status**: `needs-triage`, `in-progress`, `blocked`

## 🔒 Security

For security vulnerabilities:

1. **Do not** create a public issue
2. **Email** security@vhallatech.com
3. **Include** detailed information
4. **Wait** for acknowledgment before disclosure

## 📜 License

By contributing, you agree that your contributions will be licensed under the MIT License.

---

Thank you for contributing to PowerOrchestrator! Your efforts help make this project better for everyone. 🚀