using AutoMapper;
using FluentAssertions;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using PowerOrchestrator.API.Controllers;
using PowerOrchestrator.API.DTOs;
using PowerOrchestrator.API.Mapping;
using PowerOrchestrator.API.Validators;
using PowerOrchestrator.Application.Interfaces;
using PowerOrchestrator.Application.Interfaces.Repositories;
using PowerOrchestrator.Domain.Entities;

namespace PowerOrchestrator.UnitTests;

/// <summary>
/// Unit tests for Phase 1 API functionality
/// </summary>
public class Phase1ApiTests
{
    private readonly Mock<IUnitOfWork> _mockUnitOfWork;
    private readonly Mock<IScriptRepository> _mockScriptRepository;
    private readonly Mock<IExecutionRepository> _mockExecutionRepository;
    private readonly Mock<ILogger<ScriptsController>> _mockLogger;
    private readonly IMapper _mapper;
    private readonly ScriptsController _scriptsController;
    private readonly CreateScriptDtoValidator _createValidator;
    private readonly UpdateScriptDtoValidator _updateValidator;

    public Phase1ApiTests()
    {
        _mockUnitOfWork = new Mock<IUnitOfWork>();
        _mockScriptRepository = new Mock<IScriptRepository>();
        _mockExecutionRepository = new Mock<IExecutionRepository>();
        _mockLogger = new Mock<ILogger<ScriptsController>>();

        // Setup AutoMapper
        var mapperConfig = new MapperConfiguration(cfg =>
        {
            cfg.AddProfile<ScriptMappingProfile>();
            cfg.AddProfile<ExecutionMappingProfile>();
        });
        _mapper = mapperConfig.CreateMapper();

        // Setup repository mocks
        _mockUnitOfWork.Setup(x => x.Scripts).Returns(_mockScriptRepository.Object);
        _mockUnitOfWork.Setup(x => x.Executions).Returns(_mockExecutionRepository.Object);

        _scriptsController = new ScriptsController(_mockUnitOfWork.Object, _mapper, _mockLogger.Object);
        _createValidator = new CreateScriptDtoValidator();
        _updateValidator = new UpdateScriptDtoValidator();
    }

    [Fact]
    public void AutoMapper_Configuration_ShouldBeValid()
    {
        // Act & Assert
        var mapperConfig = new MapperConfiguration(cfg =>
        {
            cfg.AddProfile<ScriptMappingProfile>();
            cfg.AddProfile<ExecutionMappingProfile>();
        });

        mapperConfig.AssertConfigurationIsValid();
    }

    [Fact]
    public void CreateScriptDto_Validation_ShouldSucceed_WithValidData()
    {
        // Arrange
        var dto = new CreateScriptDto
        {
            Name = "Test Script",
            Description = "A test script",
            Content = "Write-Host 'Hello World'",
            Version = "1.0.0",
            TimeoutSeconds = 300,
            RequiredPowerShellVersion = "5.1"
        };

        // Act
        var result = _createValidator.Validate(dto);

        // Assert
        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    [Fact]
    public void CreateScriptDto_Validation_ShouldFail_WithInvalidName()
    {
        // Arrange
        var dto = new CreateScriptDto
        {
            Name = "", // Invalid: empty name
            Content = "Write-Host 'Hello World'",
            Version = "1.0.0"
        };

        // Act
        var result = _createValidator.Validate(dto);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(CreateScriptDto.Name));
    }

    [Fact]
    public void CreateScriptDto_Validation_ShouldFail_WithInvalidVersion()
    {
        // Arrange
        var dto = new CreateScriptDto
        {
            Name = "Test Script",
            Content = "Write-Host 'Hello World'",
            Version = "invalid-version" // Invalid: not in X.Y.Z format
        };

        // Act
        var result = _createValidator.Validate(dto);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(CreateScriptDto.Version));
    }

    [Fact]
    public void UpdateScriptDto_Validation_ShouldSucceed_WithPartialData()
    {
        // Arrange
        var dto = new UpdateScriptDto
        {
            Name = "Updated Script Name",
            // Other fields can be null for partial updates
        };

        // Act
        var result = _updateValidator.Validate(dto);

        // Assert
        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    [Fact]
    public async Task GetScripts_ShouldReturnOkResult_WithScriptDtos()
    {
        // Arrange
        var scripts = new List<Script>
        {
            new Script
            {
                Id = Guid.NewGuid(),
                Name = "Test Script 1",
                Description = "Description 1",
                Content = "Write-Host 'Test 1'",
                Version = "1.0.0",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            },
            new Script
            {
                Id = Guid.NewGuid(),
                Name = "Test Script 2",
                Description = "Description 2",
                Content = "Write-Host 'Test 2'",
                Version = "1.0.0",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            }
        };

        _mockScriptRepository.Setup(x => x.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(scripts);

        // Act
        var result = await _scriptsController.GetScripts();

        // Assert
        result.Should().NotBeNull();
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var scriptDtos = okResult.Value.Should().BeAssignableTo<IEnumerable<ScriptDto>>().Subject;
        scriptDtos.Should().HaveCount(2);
        scriptDtos.First().Name.Should().Be("Test Script 1");
    }

    [Fact]
    public async Task GetScript_ShouldReturnNotFound_WhenScriptDoesNotExist()
    {
        // Arrange
        var scriptId = Guid.NewGuid();
        _mockScriptRepository.Setup(x => x.GetByIdAsync(scriptId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Script?)null);

        // Act
        var result = await _scriptsController.GetScript(scriptId);

        // Assert
        result.Should().NotBeNull();
        result.Result.Should().BeOfType<NotFoundResult>();
    }

    [Fact]
    public async Task CreateScript_ShouldReturnCreatedResult_WithValidData()
    {
        // Arrange
        var createDto = new CreateScriptDto
        {
            Name = "New Script",
            Description = "A new test script",
            Content = "Write-Host 'Hello New World'",
            Version = "1.0.0",
            TimeoutSeconds = 300,
            RequiredPowerShellVersion = "5.1"
        };

        var createdScript = new Script
        {
            Id = Guid.NewGuid(),
            Name = createDto.Name,
            Description = createDto.Description,
            Content = createDto.Content,
            Version = createDto.Version,
            TimeoutSeconds = createDto.TimeoutSeconds,
            RequiredPowerShellVersion = createDto.RequiredPowerShellVersion,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _mockScriptRepository.Setup(x => x.AddAsync(It.IsAny<Script>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(createdScript);

        _mockUnitOfWork.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        // Act
        var result = await _scriptsController.CreateScript(createDto);

        // Assert
        result.Should().NotBeNull();
        var createdResult = result.Result.Should().BeOfType<CreatedAtActionResult>().Subject;
        var scriptDto = createdResult.Value.Should().BeAssignableTo<ScriptDto>().Subject;
        scriptDto.Name.Should().Be(createDto.Name);
        scriptDto.Description.Should().Be(createDto.Description);
    }

    [Fact]
    public void ScriptDto_Mapping_ShouldMapCorrectly()
    {
        // Arrange
        var script = new Script
        {
            Id = Guid.NewGuid(),
            Name = "Test Script",
            Description = "Test Description",
            Content = "Write-Host 'Test'",
            Version = "1.0.0",
            Tags = "test,script",
            IsActive = true,
            TimeoutSeconds = 300,
            RequiredPowerShellVersion = "5.1",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            CreatedBy = "test-user",
            UpdatedBy = "test-user"
        };

        // Act
        var dto = _mapper.Map<ScriptDto>(script);

        // Assert
        dto.Should().NotBeNull();
        dto.Id.Should().Be(script.Id);
        dto.Name.Should().Be(script.Name);
        dto.Description.Should().Be(script.Description);
        dto.Content.Should().Be(script.Content);
        dto.Version.Should().Be(script.Version);
        dto.Tags.Should().Be(script.Tags);
        dto.IsActive.Should().Be(script.IsActive);
        dto.TimeoutSeconds.Should().Be(script.TimeoutSeconds);
        dto.RequiredPowerShellVersion.Should().Be(script.RequiredPowerShellVersion);
        dto.CreatedAt.Should().Be(script.CreatedAt);
        dto.UpdatedAt.Should().Be(script.UpdatedAt);
        dto.CreatedBy.Should().Be(script.CreatedBy);
        dto.UpdatedBy.Should().Be(script.UpdatedBy);
    }
}