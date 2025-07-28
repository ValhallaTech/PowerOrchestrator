# Contributing to PowerOrchestrator

Thank you for your interest in contributing to PowerOrchestrator! This document provides guidelines and standards for contributing to the project.

## Development Workflow

### 1. Fork and Branch

1. Fork the repository on GitHub
2. Create a feature branch from `main`:
   ```bash
   git checkout -b feature/your-feature-name
   ```

### 2. Development Standards

#### Code Quality

- Follow C# coding conventions and .NET best practices
- Write meaningful commit messages
- Keep commits focused and atomic
- Ensure all tests pass before submitting

#### Testing Requirements

- Write unit tests for new features and bug fixes
- Maintain or improve test coverage
- Run the full test suite before submitting:
  ```bash
  dotnet test --collect:"XPlat Code Coverage"
  ```

#### Documentation

- Update documentation for new features
- Include XML documentation for public APIs
- Update README.md if necessary

### 3. Pull Request Process

1. **Ensure your changes are ready:**
   - All tests pass
   - Code follows project conventions
   - Documentation is updated
   - No merge conflicts with `main`

2. **Create a Pull Request:**
   - Use a descriptive title
   - Provide a detailed description of changes
   - Reference any related issues
   - Add appropriate labels

3. **Code Review:**
   - Address reviewer feedback promptly
   - Keep discussions focused and professional
   - Update your branch as needed

4. **Merge:**
   - Squash commits if requested
   - Ensure CI/CD checks pass
   - Maintainer will merge when ready

## Code Standards

### C# Conventions

- Use PascalCase for public members
- Use camelCase for private members and parameters
- Use meaningful variable and method names
- Prefer explicit types over `var` for clarity
- Follow SOLID principles

### Architecture Guidelines

- Maintain clean architecture separation
- Follow CQRS patterns for application layer
- Use dependency injection appropriately
- Implement proper error handling and logging

### Performance Considerations

- Use async/await for I/O operations
- Implement proper caching strategies
- Consider memory usage and disposal patterns
- Profile performance-critical code paths

## Development Environment

Ensure you have set up your development environment according to the [Setup Guide](setup.md).

### Required Tools

- **.NET 8 SDK**
- **Docker** and Docker Compose
- **IDE**: Visual Studio 2022, VS Code, or JetBrains Rider
- **Git** for version control

### Recommended Extensions

For Visual Studio Code:
- C# Dev Kit
- Docker
- GitLens
- SonarLint

## Project Structure

Understanding the project structure will help you navigate and contribute effectively:

```
src/
├── PowerOrchestrator.MAUI/          # MAUI UI Application
├── PowerOrchestrator.API/           # ASP.NET Core Web API
├── PowerOrchestrator.Application/   # Application Layer (CQRS, MediatR)
├── PowerOrchestrator.Domain/        # Domain Entities and Business Logic
├── PowerOrchestrator.Infrastructure/ # Data Access and External Services
└── PowerOrchestrator.Identity/      # Authentication and Authorization

tests/
├── PowerOrchestrator.UnitTests/     # Unit Tests
├── PowerOrchestrator.IntegrationTests/ # Integration Tests
└── PowerOrchestrator.LoadTests/     # Performance/Load Tests
```

## Issue Guidelines

### Reporting Bugs

When reporting bugs, please include:

- **Environment**: OS, .NET version, Docker version
- **Steps to reproduce** the issue
- **Expected behavior**
- **Actual behavior**
- **Error messages** or stack traces
- **Screenshots** if applicable

### Feature Requests

For feature requests, please include:

- **Problem statement**: What problem does this solve?
- **Proposed solution**: How should it work?
- **Alternatives considered**: What other options did you consider?
- **Additional context**: Screenshots, mockups, related issues

### Labels

We use the following labels to categorize issues:

- `bug`: Something isn't working
- `enhancement`: New feature or request
- `documentation`: Improvements or additions to documentation
- `good first issue`: Good for newcomers
- `help wanted`: Extra attention is needed
- `priority/high`: High priority issues
- `status/in-progress`: Currently being worked on

## Security

### Reporting Security Vulnerabilities

Please do not report security vulnerabilities through public GitHub issues. Instead:

1. Email security concerns to: security@vhallatech.com
2. Include detailed information about the vulnerability
3. Allow time for assessment and patching before disclosure

### Security Guidelines

- Never commit secrets, passwords, or API keys
- Use secure coding practices
- Validate all inputs and sanitize outputs
- Follow OWASP security guidelines

## Communication

### Channels

- **GitHub Issues**: Bug reports and feature requests
- **GitHub Discussions**: General questions and community discussions
- **Pull Requests**: Code reviews and technical discussions

### Guidelines

- Be respectful and professional
- Use clear, concise language
- Search existing issues before creating new ones
- Tag relevant maintainers when appropriate
- Follow the Code of Conduct

## Recognition

Contributors who make significant contributions to the project will be:

- Listed in the CONTRIBUTORS.md file
- Mentioned in release notes
- Invited to join the core team (for ongoing contributors)

## Getting Help

If you need help or have questions:

1. Check the [documentation](../README.md)
2. Search existing [GitHub Issues](https://github.com/ValhallaTech/PowerOrchestrator/issues)
3. Ask in [GitHub Discussions](https://github.com/ValhallaTech/PowerOrchestrator/discussions)
4. Review the [setup guide](setup.md)

## Code of Conduct

This project adheres to a Code of Conduct. By participating, you are expected to uphold this code. Please report unacceptable behavior to the project maintainers.

---

Thank you for contributing to PowerOrchestrator! Your efforts help make this project better for everyone. 🚀