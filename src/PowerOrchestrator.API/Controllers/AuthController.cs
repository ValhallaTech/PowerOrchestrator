using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using PowerOrchestrator.API.DTOs.Identity;
using PowerOrchestrator.Domain.Entities;
using PowerOrchestrator.Identity.Services;
using PowerOrchestrator.Infrastructure.Identity;
using System.Security.Claims;
using ILogger = Serilog.ILogger;

namespace PowerOrchestrator.API.Controllers;

/// <summary>
/// Authentication controller for user login, registration, and token management
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class AuthController : ControllerBase
{
    private readonly UserManager<User> _userManager;
    private readonly SignInManager<User> _signInManager;
    private readonly RoleManager<Role> _roleManager;
    private readonly IJwtTokenService _jwtTokenService;
    private readonly IMfaService _mfaService;
    private readonly IUserRepository _userRepository;
    private readonly ILogger _logger;

    /// <summary>
    /// Initializes a new instance of the AuthController class
    /// </summary>
    public AuthController(
        UserManager<User> userManager,
        SignInManager<User> signInManager,
        RoleManager<Role> roleManager,
        IJwtTokenService jwtTokenService,
        IMfaService mfaService,
        IUserRepository userRepository,
        ILogger logger)
    {
        _userManager = userManager ?? throw new ArgumentNullException(nameof(userManager));
        _signInManager = signInManager ?? throw new ArgumentNullException(nameof(signInManager));
        _roleManager = roleManager ?? throw new ArgumentNullException(nameof(roleManager));
        _jwtTokenService = jwtTokenService ?? throw new ArgumentNullException(nameof(jwtTokenService));
        _mfaService = mfaService ?? throw new ArgumentNullException(nameof(mfaService));
        _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Authenticates a user and returns a JWT token
    /// </summary>
    /// <param name="request">The login request</param>
    /// <returns>Login response with JWT token</returns>
    [HttpPost("login")]
    [ProducesResponseType(typeof(LoginResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(object), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(object), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> LoginAsync([FromBody] LoginRequest request)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var user = await _userManager.FindByEmailAsync(request.Email);
            if (user == null)
            {
                _logger.Warning("Login attempt for non-existent user: {Email}", request.Email);
                return Unauthorized(new { Message = "Invalid email or password" });
            }

            // Check if user is locked
            if (user.LockedUntil.HasValue && user.LockedUntil > DateTime.UtcNow)
            {
                _logger.Warning("Login attempt for locked user: {Email}, locked until: {LockedUntil}", 
                    request.Email, user.LockedUntil);
                return Unauthorized(new { Message = $"Account is locked until {user.LockedUntil}" });
            }

            // Check password
            var passwordCheck = await _userManager.CheckPasswordAsync(user, request.Password);
            if (!passwordCheck)
            {
                await _userRepository.IncrementFailedLoginAttemptsAsync(user.Id);
                
                // Lock account after 5 failed attempts
                if (user.FailedLoginAttempts >= 4) // Will be 5 after increment
                {
                    await _userRepository.LockUserAsync(user.Id, DateTime.UtcNow.AddMinutes(15));
                    _logger.Warning("User account locked due to failed login attempts: {Email}", request.Email);
                    return Unauthorized(new { Message = "Account locked due to too many failed login attempts" });
                }

                _logger.Warning("Invalid password for user: {Email}", request.Email);
                return Unauthorized(new { Message = "Invalid email or password" });
            }

            // Check MFA if enabled
            if (user.IsMfaEnabled)
            {
                if (string.IsNullOrWhiteSpace(request.MfaCode))
                {
                    return BadRequest(new { Message = "MFA code is required", RequiresMfa = true });
                }

                if (!_mfaService.ValidateCode(user.MfaSecret!, request.MfaCode))
                {
                    _logger.Warning("Invalid MFA code for user: {Email}", request.Email);
                    return Unauthorized(new { Message = "Invalid MFA code" });
                }
            }

            // Reset failed login attempts on successful login
            await _userRepository.ResetFailedLoginAttemptsAsync(user.Id);
            await _userRepository.UpdateLastLoginAsync(user.Id, GetClientIpAddress());

            // Get user roles and permissions
            var roles = await _userManager.GetRolesAsync(user);
            var permissions = new List<string>();
            
            foreach (var roleName in roles)
            {
                var role = await _roleManager.FindByNameAsync(roleName);
                if (role != null && !string.IsNullOrEmpty(role.Permissions))
                {
                    var rolePermissions = Newtonsoft.Json.JsonConvert.DeserializeObject<List<string>>(role.Permissions);
                    if (rolePermissions != null)
                    {
                        permissions.AddRange(rolePermissions);
                    }
                }
            }

            // Generate JWT token
            var token = await _jwtTokenService.GenerateTokenAsync(
                user.Id, 
                user.Email!, 
                roles, 
                permissions.Distinct());

            var response = new LoginResponse
            {
                AccessToken = token.AccessToken,
                RefreshToken = token.RefreshToken,
                ExpiresAt = token.ExpiresAt,
                TokenType = token.TokenType,
                User = new UserInfo
                {
                    Id = user.Id,
                    Email = user.Email!,
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    FullName = user.FullName,
                    Roles = roles.ToList(),
                    Permissions = permissions.Distinct().ToList(),
                    IsMfaEnabled = user.IsMfaEnabled,
                    LastLoginAt = user.LastLoginAt
                }
            };

            _logger.Information("User logged in successfully: {Email}", request.Email);
            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Error during login for user: {Email}", request.Email);
            return StatusCode(500, new { Message = "An error occurred during login" });
        }
    }

    /// <summary>
    /// Registers a new user account
    /// </summary>
    /// <param name="request">The registration request</param>
    /// <returns>Registration response</returns>
    [HttpPost("register")]
    [ProducesResponseType(typeof(RegisterResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(object), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> RegisterAsync([FromBody] RegisterRequest request)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var existingUser = await _userManager.FindByEmailAsync(request.Email);
            if (existingUser != null)
            {
                return BadRequest(new RegisterResponse
                {
                    Success = false,
                    Errors = new List<string> { "User with this email already exists" }
                });
            }

            var user = new User
            {
                UserName = request.Email,
                Email = request.Email,
                FirstName = request.FirstName,
                LastName = request.LastName,
                EmailConfirmed = true // For now, skip email confirmation
            };

            var result = await _userManager.CreateAsync(user, request.Password);
            if (!result.Succeeded)
            {
                return BadRequest(new RegisterResponse
                {
                    Success = false,
                    Errors = result.Errors.Select(e => e.Description).ToList()
                });
            }

            // Assign default User role
            if (await _roleManager.RoleExistsAsync("User"))
            {
                await _userManager.AddToRoleAsync(user, "User");
            }

            _logger.Information("New user registered: {Email}", request.Email);

            return Ok(new RegisterResponse
            {
                Success = true,
                UserId = user.Id,
                Message = "User registered successfully"
            });
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Error during registration for user: {Email}", request.Email);
            return StatusCode(500, new RegisterResponse
            {
                Success = false,
                Errors = new List<string> { "An error occurred during registration" }
            });
        }
    }

    /// <summary>
    /// Sets up Multi-Factor Authentication for the current user
    /// </summary>
    /// <returns>MFA setup information</returns>
    [HttpPost("mfa/setup")]
    [Authorize]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> SetupMfaAsync()
    {
        try
        {
            var userId = GetCurrentUserId();
            var user = await _userManager.FindByIdAsync(userId.ToString());
            
            if (user == null)
            {
                return Unauthorized();
            }

            if (user.IsMfaEnabled)
            {
                return BadRequest(new { Message = "MFA is already enabled for this user" });
            }

            var secret = _mfaService.GenerateSecret();
            var qrCodeUrl = _mfaService.GenerateQrCodeUrl(user.Email!, secret);

            // Save the secret but don't enable MFA yet (will be enabled after verification)
            await _userRepository.SaveMfaSecretAsync(user.Id, secret);

            _logger.Information("MFA setup initiated for user: {UserId}", userId);

            return Ok(new
            {
                Secret = secret,
                QrCodeUrl = qrCodeUrl,
                Message = "Scan the QR code with your authenticator app and verify with a code to complete setup"
            });
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Error during MFA setup for user: {UserId}", GetCurrentUserId());
            return StatusCode(500, new { Message = "An error occurred during MFA setup" });
        }
    }

    /// <summary>
    /// Verifies MFA setup with a TOTP code
    /// </summary>
    /// <param name="code">The TOTP code from the authenticator app</param>
    /// <returns>Verification result</returns>
    [HttpPost("mfa/verify")]
    [Authorize]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> VerifyMfaAsync([FromBody] string code)
    {
        try
        {
            var userId = GetCurrentUserId();
            var user = await _userManager.FindByIdAsync(userId.ToString());
            
            if (user == null)
            {
                return Unauthorized();
            }

            if (string.IsNullOrWhiteSpace(user.MfaSecret))
            {
                return BadRequest(new { Message = "MFA setup not initiated. Please setup MFA first." });
            }

            if (!_mfaService.ValidateCode(user.MfaSecret, code))
            {
                return BadRequest(new { Message = "Invalid verification code" });
            }

            // Enable MFA for the user
            user.IsMfaEnabled = true;
            await _userManager.UpdateAsync(user);

            _logger.Information("MFA enabled for user: {UserId}", userId);

            return Ok(new { Message = "MFA has been successfully enabled for your account" });
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Error during MFA verification for user: {UserId}", GetCurrentUserId());
            return StatusCode(500, new { Message = "An error occurred during MFA verification" });
        }
    }

    /// <summary>
    /// Gets current user information
    /// </summary>
    /// <returns>Current user information</returns>
    [HttpGet("me")]
    [Authorize]
    [ProducesResponseType(typeof(UserInfo), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetCurrentUserAsync()
    {
        try
        {
            var userId = GetCurrentUserId();
            var user = await _userManager.FindByIdAsync(userId.ToString());
            
            if (user == null)
            {
                return Unauthorized();
            }

            var roles = await _userManager.GetRolesAsync(user);
            var permissions = new List<string>();
            
            foreach (var roleName in roles)
            {
                var role = await _roleManager.FindByNameAsync(roleName);
                if (role != null && !string.IsNullOrEmpty(role.Permissions))
                {
                    var rolePermissions = Newtonsoft.Json.JsonConvert.DeserializeObject<List<string>>(role.Permissions);
                    if (rolePermissions != null)
                    {
                        permissions.AddRange(rolePermissions);
                    }
                }
            }

            var userInfo = new UserInfo
            {
                Id = user.Id,
                Email = user.Email!,
                FirstName = user.FirstName,
                LastName = user.LastName,
                FullName = user.FullName,
                Roles = roles.ToList(),
                Permissions = permissions.Distinct().ToList(),
                IsMfaEnabled = user.IsMfaEnabled,
                LastLoginAt = user.LastLoginAt
            };

            return Ok(userInfo);
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Error getting current user info for user: {UserId}", GetCurrentUserId());
            return StatusCode(500, new { Message = "An error occurred while retrieving user information" });
        }
    }

    /// <summary>
    /// Gets the current user ID from the JWT token
    /// </summary>
    /// <returns>Current user ID</returns>
    private Guid GetCurrentUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return Guid.Parse(userIdClaim!);
    }

    /// <summary>
    /// Gets the client IP address
    /// </summary>
    /// <returns>Client IP address</returns>
    private string? GetClientIpAddress()
    {
        return Request.Headers["X-Forwarded-For"].FirstOrDefault() 
               ?? HttpContext.Connection.RemoteIpAddress?.ToString();
    }
}