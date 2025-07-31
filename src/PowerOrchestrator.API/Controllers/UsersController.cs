using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PowerOrchestrator.API.DTOs.Identity;
using Microsoft.AspNetCore.Identity;
using PowerOrchestrator.Domain.Entities;
using AutoMapper;
using FluentValidation;

namespace PowerOrchestrator.API.Controllers;

/// <summary>
/// Controller for managing users
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class UsersController : ControllerBase
{
    private readonly UserManager<User> _userManager;
    private readonly IMapper _mapper;
    private readonly ILogger<UsersController> _logger;

    /// <summary>
    /// Initializes a new instance of the UsersController
    /// </summary>
    public UsersController(
        UserManager<User> userManager,
        IMapper mapper,
        ILogger<UsersController> logger)
    {
        _userManager = userManager;
        _mapper = mapper;
        _logger = logger;
    }

    /// <summary>
    /// Get all users
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<UserDto>>> GetUsers()
    {
        try
        {
            await Task.CompletedTask; // Make it properly async
            var users = _userManager.Users.ToList();
            var userDtos = users.Select(u => new UserDto
            {
                Id = u.Id.ToString(),
                UserName = u.UserName ?? string.Empty,
                Email = u.Email ?? string.Empty,
                FirstName = u.FirstName,
                LastName = u.LastName,
                EmailConfirmed = u.EmailConfirmed,
                LastLoginAt = u.LastLoginAt,
                IsActive = !u.LockoutEnabled || u.LockoutEnd == null || u.LockoutEnd <= DateTimeOffset.UtcNow
            });

            return Ok(userDtos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving users");
            return StatusCode(500, "An error occurred while retrieving users");
        }
    }

    /// <summary>
    /// Get user by ID
    /// </summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<UserDto>> GetUser(string id)
    {
        try
        {
            if (!Guid.TryParse(id, out var userId))
            {
                return BadRequest("Invalid user ID format");
            }

            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            var userDto = new UserDto
            {
                Id = user.Id.ToString(),
                UserName = user.UserName ?? string.Empty,
                Email = user.Email ?? string.Empty,
                FirstName = user.FirstName,
                LastName = user.LastName,
                EmailConfirmed = user.EmailConfirmed,
                LastLoginAt = user.LastLoginAt,
                IsActive = !user.LockoutEnabled || user.LockoutEnd == null || user.LockoutEnd <= DateTimeOffset.UtcNow
            };

            return Ok(userDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving user {UserId}", id);
            return StatusCode(500, "An error occurred while retrieving the user");
        }
    }

    /// <summary>
    /// Create a new user
    /// </summary>
    [HttpPost]
    [Authorize(Roles = "Administrator")]
    public async Task<ActionResult<UserDto>> CreateUser([FromBody] CreateUserDto createUserDto)
    {
        try
        {
            var user = new User
            {
                UserName = createUserDto.UserName,
                Email = createUserDto.Email,
                FirstName = createUserDto.FirstName,
                LastName = createUserDto.LastName,
                EmailConfirmed = true // Admin-created users are auto-confirmed
            };

            var result = await _userManager.CreateAsync(user, createUserDto.Password);
            if (!result.Succeeded)
            {
                return BadRequest(result.Errors);
            }

            var userDto = new UserDto
            {
                Id = user.Id.ToString(),
                UserName = user.UserName ?? string.Empty,
                Email = user.Email ?? string.Empty,
                FirstName = user.FirstName,
                LastName = user.LastName,
                EmailConfirmed = user.EmailConfirmed,
                IsActive = true
            };

            return CreatedAtAction(nameof(GetUser), new { id = user.Id }, userDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating user");
            return StatusCode(500, "An error occurred while creating the user");
        }
    }

    /// <summary>
    /// Update an existing user
    /// </summary>
    [HttpPut("{id}")]
    [Authorize(Roles = "Administrator")]
    public async Task<IActionResult> UpdateUser(string id, [FromBody] UpdateUserDto updateUserDto)
    {
        try
        {
            if (!Guid.TryParse(id, out var userId))
            {
                return BadRequest("Invalid user ID format");
            }

            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            user.FirstName = updateUserDto.FirstName;
            user.LastName = updateUserDto.LastName;
            user.Email = updateUserDto.Email;
            user.UpdatedAt = DateTime.UtcNow;

            var result = await _userManager.UpdateAsync(user);
            if (!result.Succeeded)
            {
                return BadRequest(result.Errors);
            }

            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating user {UserId}", id);
            return StatusCode(500, "An error occurred while updating the user");
        }
    }

    /// <summary>
    /// Delete a user
    /// </summary>
    [HttpDelete("{id}")]
    [Authorize(Roles = "Administrator")]
    public async Task<IActionResult> DeleteUser(string id)
    {
        try
        {
            if (!Guid.TryParse(id, out var userId))
            {
                return BadRequest("Invalid user ID format");
            }

            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            var result = await _userManager.DeleteAsync(user);
            if (!result.Succeeded)
            {
                return BadRequest(result.Errors);
            }

            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting user {UserId}", id);
            return StatusCode(500, "An error occurred while deleting the user");
        }
    }
}