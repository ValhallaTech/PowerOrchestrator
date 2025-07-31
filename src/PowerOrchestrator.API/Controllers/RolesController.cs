using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PowerOrchestrator.API.DTOs.Identity;
using Microsoft.AspNetCore.Identity;
using PowerOrchestrator.Domain.Entities;
using AutoMapper;

namespace PowerOrchestrator.API.Controllers;

/// <summary>
/// Controller for managing roles
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class RolesController : ControllerBase
{
    private readonly RoleManager<Role> _roleManager;
    private readonly IMapper _mapper;
    private readonly ILogger<RolesController> _logger;

    /// <summary>
    /// Initializes a new instance of the RolesController
    /// </summary>
    public RolesController(
        RoleManager<Role> roleManager,
        IMapper mapper,
        ILogger<RolesController> logger)
    {
        _roleManager = roleManager;
        _mapper = mapper;
        _logger = logger;
    }

    /// <summary>
    /// Get all roles
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<RoleDto>>> GetRoles()
    {
        try
        {
            await Task.CompletedTask; // Make it properly async
            var roles = _roleManager.Roles.ToList();
            var roleDtos = roles.Select(r => new RoleDto
            {
                Id = r.Id.ToString(),
                Name = r.Name ?? string.Empty,
                Description = r.Description,
                IsSystemRole = r.IsSystemRole,
                CreatedAt = r.CreatedAt
            });

            return Ok(roleDtos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving roles");
            return StatusCode(500, "An error occurred while retrieving roles");
        }
    }

    /// <summary>
    /// Get role by ID
    /// </summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<RoleDto>> GetRole(string id)
    {
        try
        {
            if (!Guid.TryParse(id, out var roleId))
            {
                return BadRequest("Invalid role ID format");
            }

            var role = await _roleManager.FindByIdAsync(id);
            if (role == null)
            {
                return NotFound();
            }

            var roleDto = new RoleDto
            {
                Id = role.Id.ToString(),
                Name = role.Name ?? string.Empty,
                Description = role.Description,
                IsSystemRole = role.IsSystemRole,
                CreatedAt = role.CreatedAt
            };

            return Ok(roleDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving role {RoleId}", id);
            return StatusCode(500, "An error occurred while retrieving the role");
        }
    }

    /// <summary>
    /// Create a new role
    /// </summary>
    [HttpPost]
    [Authorize(Roles = "Administrator")]
    public async Task<ActionResult<RoleDto>> CreateRole([FromBody] CreateRoleDto createRoleDto)
    {
        try
        {
            var role = new Role
            {
                Name = createRoleDto.Name,
                Description = createRoleDto.Description,
                IsSystemRole = false, // User-created roles are never system roles
                Permissions = "[]" // Default empty permissions
            };

            var result = await _roleManager.CreateAsync(role);
            if (!result.Succeeded)
            {
                return BadRequest(result.Errors);
            }

            var roleDto = new RoleDto
            {
                Id = role.Id.ToString(),
                Name = role.Name ?? string.Empty,
                Description = role.Description,
                IsSystemRole = role.IsSystemRole,
                CreatedAt = role.CreatedAt
            };

            return CreatedAtAction(nameof(GetRole), new { id = role.Id }, roleDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating role");
            return StatusCode(500, "An error occurred while creating the role");
        }
    }

    /// <summary>
    /// Update an existing role
    /// </summary>
    [HttpPut("{id}")]
    [Authorize(Roles = "Administrator")]
    public async Task<IActionResult> UpdateRole(string id, [FromBody] UpdateRoleDto updateRoleDto)
    {
        try
        {
            if (!Guid.TryParse(id, out var roleId))
            {
                return BadRequest("Invalid role ID format");
            }

            var role = await _roleManager.FindByIdAsync(id);
            if (role == null)
            {
                return NotFound();
            }

            if (role.IsSystemRole)
            {
                return BadRequest("System roles cannot be modified");
            }

            role.Description = updateRoleDto.Description;
            role.UpdatedAt = DateTime.UtcNow;

            var result = await _roleManager.UpdateAsync(role);
            if (!result.Succeeded)
            {
                return BadRequest(result.Errors);
            }

            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating role {RoleId}", id);
            return StatusCode(500, "An error occurred while updating the role");
        }
    }

    /// <summary>
    /// Delete a role
    /// </summary>
    [HttpDelete("{id}")]
    [Authorize(Roles = "Administrator")]
    public async Task<IActionResult> DeleteRole(string id)
    {
        try
        {
            if (!Guid.TryParse(id, out var roleId))
            {
                return BadRequest("Invalid role ID format");
            }

            var role = await _roleManager.FindByIdAsync(id);
            if (role == null)
            {
                return NotFound();
            }

            if (role.IsSystemRole)
            {
                return BadRequest("System roles cannot be deleted");
            }

            var result = await _roleManager.DeleteAsync(role);
            if (!result.Succeeded)
            {
                return BadRequest(result.Errors);
            }

            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting role {RoleId}", id);
            return StatusCode(500, "An error occurred while deleting the role");
        }
    }
}