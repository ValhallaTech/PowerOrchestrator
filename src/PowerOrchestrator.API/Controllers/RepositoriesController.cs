using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PowerOrchestrator.API.DTOs;
using PowerOrchestrator.Application.Interfaces.Repositories;
using PowerOrchestrator.Domain.Entities;
using PowerOrchestrator.Domain.ValueObjects;
using AutoMapper;

namespace PowerOrchestrator.API.Controllers;

/// <summary>
/// Controller for managing GitHub repositories
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class RepositoriesController : ControllerBase
{
    private readonly IGitHubRepositoryRepository _repositoryRepository;
    private readonly IMapper _mapper;
    private readonly ILogger<RepositoriesController> _logger;

    /// <summary>
    /// Initializes a new instance of the RepositoriesController
    /// </summary>
    public RepositoriesController(
        IGitHubRepositoryRepository repositoryRepository,
        IMapper mapper,
        ILogger<RepositoriesController> logger)
    {
        _repositoryRepository = repositoryRepository;
        _mapper = mapper;
        _logger = logger;
    }

    /// <summary>
    /// Get all repositories
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<GitHubRepositoryDto>>> GetRepositories()
    {
        try
        {
            var repositories = await _repositoryRepository.GetAllAsync();
            var repositoryDtos = repositories.Select(r => new GitHubRepositoryDto
            {
                Id = r.Id,
                Owner = r.Owner,
                Name = r.Name,
                FullName = r.FullName,
                Description = r.Description,
                IsPrivate = r.IsPrivate,
                DefaultBranch = r.DefaultBranch,
                LastSyncAt = r.LastSyncAt,
                Status = r.Status,
                ScriptCount = r.Scripts?.Count ?? 0,
                CreatedAt = r.CreatedAt,
                UpdatedAt = r.UpdatedAt
            });

            return Ok(repositoryDtos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving repositories");
            return StatusCode(500, "An error occurred while retrieving repositories");
        }
    }

    /// <summary>
    /// Get repository by ID
    /// </summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<GitHubRepositoryDto>> GetRepository(Guid id)
    {
        try
        {
            var repository = await _repositoryRepository.GetByIdAsync(id);
            if (repository == null)
            {
                return NotFound();
            }

            var repositoryDto = new GitHubRepositoryDto
            {
                Id = repository.Id,
                Owner = repository.Owner,
                Name = repository.Name,
                FullName = repository.FullName,
                Description = repository.Description,
                IsPrivate = repository.IsPrivate,
                DefaultBranch = repository.DefaultBranch,
                LastSyncAt = repository.LastSyncAt,
                Status = repository.Status,
                ScriptCount = repository.Scripts?.Count ?? 0,
                CreatedAt = repository.CreatedAt,
                UpdatedAt = repository.UpdatedAt
            };

            return Ok(repositoryDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving repository {RepositoryId}", id);
            return StatusCode(500, "An error occurred while retrieving the repository");
        }
    }

    /// <summary>
    /// Create a new repository
    /// </summary>
    [HttpPost]
    [Authorize(Roles = "Administrator,PowerUser")]
    public async Task<ActionResult<GitHubRepositoryDto>> CreateRepository([FromBody] CreateGitHubRepositoryDto createRepositoryDto)
    {
        try
        {
            var repository = new GitHubRepository
            {
                Owner = createRepositoryDto.Owner,
                Name = createRepositoryDto.Name,
                FullName = $"{createRepositoryDto.Owner}/{createRepositoryDto.Name}",
                Description = createRepositoryDto.Description,
                IsPrivate = createRepositoryDto.IsPrivate,
                DefaultBranch = createRepositoryDto.DefaultBranch ?? "main",
                Status = RepositoryStatus.Active,
                Configuration = "{}", // Default empty configuration 
                CreatedBy = "System", // TODO: Get from current user context
                UpdatedBy = "System"  // TODO: Get from current user context
            };

            var createdRepository = await _repositoryRepository.AddAsync(repository);

            var repositoryDto = new GitHubRepositoryDto
            {
                Id = createdRepository.Id,
                Owner = createdRepository.Owner,
                Name = createdRepository.Name,
                FullName = createdRepository.FullName,
                Description = createdRepository.Description,
                IsPrivate = createdRepository.IsPrivate,
                DefaultBranch = createdRepository.DefaultBranch,
                LastSyncAt = createdRepository.LastSyncAt,
                Status = createdRepository.Status,
                ScriptCount = 0,
                CreatedAt = createdRepository.CreatedAt,
                UpdatedAt = createdRepository.UpdatedAt
            };

            return CreatedAtAction(nameof(GetRepository), new { id = createdRepository.Id }, repositoryDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating repository");
            return StatusCode(500, "An error occurred while creating the repository");
        }
    }

    /// <summary>
    /// Update an existing repository
    /// </summary>
    [HttpPut("{id}")]
    [Authorize(Roles = "Administrator,PowerUser")]
    public async Task<IActionResult> UpdateRepository(Guid id, [FromBody] GitHubRepositoryDto updateRepositoryDto)
    {
        try
        {
            var repository = await _repositoryRepository.GetByIdAsync(id);
            if (repository == null)
            {
                return NotFound();
            }

            repository.Description = updateRepositoryDto.Description;
            repository.DefaultBranch = updateRepositoryDto.DefaultBranch;
            repository.Status = updateRepositoryDto.Status;
            repository.UpdatedAt = DateTime.UtcNow;
            repository.UpdatedBy = "System"; // TODO: Get from current user context

            _repositoryRepository.Update(repository);

            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating repository {RepositoryId}", id);
            return StatusCode(500, "An error occurred while updating the repository");
        }
    }

    /// <summary>
    /// Delete a repository
    /// </summary>
    [HttpDelete("{id}")]
    [Authorize(Roles = "Administrator")]
    public async Task<IActionResult> DeleteRepository(Guid id)
    {
        try
        {
            var repository = await _repositoryRepository.GetByIdAsync(id);
            if (repository == null)
            {
                return NotFound();
            }

            await _repositoryRepository.RemoveByIdAsync(id);

            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting repository {RepositoryId}", id);
            return StatusCode(500, "An error occurred while deleting the repository");
        }
    }

    /// <summary>
    /// Sync a repository with GitHub
    /// </summary>
    [HttpPost("{id}/sync")]
    [Authorize(Roles = "Administrator,PowerUser")]
    public async Task<IActionResult> SyncRepository(Guid id)
    {
        try
        {
            var repository = await _repositoryRepository.GetByIdAsync(id);
            if (repository == null)
            {
                return NotFound();
            }

            // Update sync status - use 'Active' instead of 'Syncing' which doesn't exist
            repository.Status = RepositoryStatus.Active;
            repository.LastSyncAt = DateTime.UtcNow;
            repository.UpdatedAt = DateTime.UtcNow;
            repository.UpdatedBy = "System"; // TODO: Get from current user context

            _repositoryRepository.Update(repository);

            // TODO: Trigger actual sync process (this would be handled by a background service)
            
            return Accepted(); // Return 202 since sync is an async operation
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error syncing repository {RepositoryId}", id);
            return StatusCode(500, "An error occurred while syncing the repository");
        }
    }
}