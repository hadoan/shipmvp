using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using ShipMvp.Application.Identity;
using ShipMvp.Application.Email;
using Swashbuckle.AspNetCore.Annotations;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Authorization;

namespace ShipMvp.Api.Controllers;

/// <summary>
/// Authentication endpoints for login/logout operations
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;
    private readonly IUserService _userService;
    private readonly IEmailApplicationService _emailService;
    private readonly ILogger<AuthController> _logger;

    /// <summary>
    /// Initializes a new instance of the AuthController
    /// </summary>
    /// <param name="authService">Authentication service</param>
    /// <param name="userService">User service</param>
    /// <param name="emailService">Email service</param>
    /// <param name="logger">Logger instance</param>
    public AuthController(
        IAuthService authService,
        IUserService userService,
        IEmailApplicationService emailService,
        ILogger<AuthController> logger)
    {
        _authService = authService;
        _userService = userService;
        _emailService = emailService;
        _logger = logger;
    }

    /// <summary>
    /// Authenticate user with username and password
    /// </summary>
    /// <param name="request">Login credentials</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Authentication result with token and user info</returns>
    [AllowAnonymous]
    [HttpPost("login")]
    [SwaggerOperation(
        Summary = "User login",
        Description = "Authenticates a user with email and password and returns a JWT token"
    )]
    [SwaggerResponse(200, "Login successful", typeof(AuthResultDto))]
    [SwaggerResponse(400, "Invalid login credentials")]
    [SwaggerResponse(429, "Too many login attempts")]
    public async Task<ActionResult<AuthResultDto>> LoginAsync(
        [FromBody] LoginDto request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogDebug("Login attempt for user: {Email}", request.Email);

            var result = await _authService.LoginAsync(request, cancellationToken);

            // Log the detailed login result for debugging
            _logger.LogDebug("Login result for user {Email}: Success={Success}, ErrorMessage={ErrorMessage}",
                request.Email, result.Success, result.ErrorMessage);

            if (!result.Success)
            {
                _logger.LogWarning("Login failed for user: {Email}. Reason: {ErrorMessage}",
                    request.Email, result.ErrorMessage);
                return BadRequest(result);
            }

            _logger.LogInformation("User logged in successfully: {Email}", request.Email);
            return Ok(result);
        }
        catch (Exception ex)
        {
            // Log the actual exception for debugging
            _logger.LogError(ex, "Login failed for user request. Error: {ErrorMessage}", ex.Message);

            return BadRequest(new AuthResultDto
            {
                Success = false,
                ErrorMessage = "An error occurred during login"
            });
        }
    }

    /// <summary>
    /// Logout the current user
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Success confirmation</returns>
    [HttpPost("logout")]
    [SwaggerOperation(
        Summary = "User logout",
        Description = "Logs out the current user and invalidates their token"
    )]
    [SwaggerResponse(200, "Logout successful")]
    public async Task<ActionResult> LogoutAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogDebug("Logout attempt initiated");
            await _authService.LogoutAsync(cancellationToken);
            _logger.LogInformation("User logged out successfully");
            return Ok(new { message = "Logout successful" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred during logout: {ErrorMessage}", ex.Message);
            return BadRequest(new { message = "An error occurred during logout" });
        }
    }

    /// <summary>
    /// Refresh authentication token
    /// </summary>
    /// <param name="request">Token refresh request</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>New authentication token</returns>
    [HttpPost("refresh")]
    [SwaggerOperation(
        Summary = "Refresh token",
        Description = "Refresh an existing JWT token"
    )]
    [SwaggerResponse(200, "Token refreshed successfully", typeof(AuthResultDto))]
    [SwaggerResponse(401, "Invalid token")]
    public async Task<ActionResult<AuthResultDto>> RefreshTokenAsync(
        [FromBody] RefreshTokenRequest request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogDebug("Token refresh attempt initiated");
            var result = await _authService.RefreshTokenAsync(request.Token, cancellationToken);
            _logger.LogInformation("Token refreshed successfully");
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Token refresh failed: {ErrorMessage}", ex.Message);
            return Unauthorized(new AuthResultDto
            {
                Success = false,
                ErrorMessage = "Invalid token"
            });
        }
    }

    /// <summary>
    /// Register a new user account
    /// </summary>
    /// <param name="request">User registration data</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Registration result with user info</returns>
    [AllowAnonymous]
    [HttpPost("register")]
    [SwaggerOperation(
        Summary = "User registration",
        Description = "Register a new user account and send a confirmation email"
    )]
    [SwaggerResponse(201, "Registration successful", typeof(RegisterResultDto))]
    [SwaggerResponse(400, "Invalid registration data")]
    [SwaggerResponse(409, "Username or email already exists")]
    public async Task<ActionResult<RegisterResultDto>> RegisterAsync(
        [FromBody] RegisterDto request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogDebug("Registration attempt for user: {Username}", request.Username);

            // Create the user
            var createUserRequest = new CreateUserDto
            {
                Username = request.Username,
                Name = request.Name,
                Surname = request.Surname,
                Email = request.Email,
                Password = request.Password,
                PhoneNumber = request.PhoneNumber,
                IsActive = false // User starts inactive until email is confirmed
            };

            var user = await _userService.CreateAsync(createUserRequest, cancellationToken);

            // Generate email confirmation token (simplified for this demo)
            var confirmationToken = Guid.NewGuid().ToString("N");

            // Send signup confirmation email
            var emailResult = await _emailService.SendSignupConfirmationEmailAsync(
                user.Id,
                user.Email,
                $"{user.Name} {user.Surname}",
                confirmationToken,
                cancellationToken);

            var result = new RegisterResultDto
            {
                Success = true,
                UserId = user.Id.ToString(),
                Username = user.Username,
                Email = user.Email,
                Message = "Registration successful. Please check your email to confirm your account.",
                EmailSent = emailResult.IsSuccess
            };

            if (!emailResult.IsSuccess)
            {
                _logger.LogWarning("User registered but email sending failed: {UserId}, Email error: {EmailError}",
                    user.Id, emailResult.ErrorMessage);
                result.Message += " Note: There was an issue sending the confirmation email. Please contact support.";
            }

            _logger.LogInformation("User registered successfully: {Username}, Email sent: {EmailSent}",
                request.Username, emailResult.IsSuccess);

            return CreatedAtAction(nameof(GetUserStatus), new { userId = user.Id }, result);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning("Registration failed for user: {Username}. Reason: {Reason}", request.Username, ex.Message);
            return Conflict(new RegisterResultDto
            {
                Success = false,
                ErrorMessage = ex.Message
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Registration failed for user: {Username}. Error: {ErrorMessage}", request.Username, ex.Message);
            return BadRequest(new RegisterResultDto
            {
                Success = false,
                ErrorMessage = "An error occurred during registration"
            });
        }
    }

    /// <summary>
    /// Get user registration status
    /// </summary>
    /// <param name="userId">User ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>User status information</returns>
    [HttpGet("status/{userId}")]
    [SwaggerOperation(
        Summary = "Get user status",
        Description = "Get user registration and email confirmation status"
    )]
    [SwaggerResponse(200, "User status retrieved successfully", typeof(UserStatusDto))]
    [SwaggerResponse(404, "User not found")]
    public async Task<ActionResult<UserStatusDto>> GetUserStatus(
        [FromRoute] string userId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var user = await _userService.GetByIdAsync(Guid.Parse(userId), cancellationToken);
            if (user == null)
            {
                return NotFound(new { message = "User not found" });
            }

            return Ok(new UserStatusDto
            {
                UserId = user.Id.ToString(),
                Username = user.Username,
                Email = user.Email,
                IsActive = user.IsActive,
                EmailConfirmed = user.IsActive // Simplified: using IsActive as email confirmation status
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting user status for ID: {UserId}", userId);
            return BadRequest(new { message = "An error occurred while getting user status" });
        }
    }

    /// <summary>
    /// Request model for token refresh
    /// </summary>
    public record RefreshTokenRequest
    {
        /// <summary>
        /// The token to refresh
        /// </summary>
        public string Token { get; init; } = string.Empty;
    }

    /// <summary>
    /// DTO for user registration result
    /// </summary>
    public class RegisterResultDto
    {
        /// <summary>
        /// Indicates if the registration was successful
        /// </summary>
        public bool Success { get; set; }

        /// <summary>
        /// User ID of the registered user
        /// </summary>
        public string UserId { get; set; } = string.Empty;

        /// <summary>
        /// Username of the registered user
        /// </summary>
        public string Username { get; set; } = string.Empty;

        /// <summary>
        /// Email of the registered user
        /// </summary>
        public string Email { get; set; } = string.Empty;

        /// <summary>
        /// Confirmation message or error message
        /// </summary>
        public string Message { get; set; } = string.Empty;

        /// <summary>
        /// Indicates if the confirmation email was sent successfully
        /// </summary>
        public bool EmailSent { get; set; }

        /// <summary>
        /// Error message if registration failed
        /// </summary>
        public string? ErrorMessage { get; set; }
    }

    /// <summary>
    /// DTO for user status information
    /// </summary>
    public class UserStatusDto
    {
        /// <summary>
        /// User ID
        /// </summary>
        public string UserId { get; set; } = string.Empty;

        /// <summary>
        /// Username
        /// </summary>
        public string Username { get; set; } = string.Empty;

        /// <summary>
        /// Email address
        /// </summary>
        public string Email { get; set; } = string.Empty;

        /// <summary>
        /// Indicates if the user account is active
        /// </summary>
        public bool IsActive { get; set; }

        /// <summary>
        /// Indicates if the email is confirmed
        /// </summary>
        public bool EmailConfirmed { get; set; }
    }

    /// <summary>
    /// DTO for user registration
    /// </summary>
    public record RegisterDto
    {
        /// <summary>
        /// Username for the new account
        /// </summary>
        [Required]
        [StringLength(50, MinimumLength = 3)]
        public string Username { get; init; } = string.Empty;

        /// <summary>
        /// User's first name
        /// </summary>
        [Required]
        [StringLength(100)]
        public string Name { get; init; } = string.Empty;

        /// <summary>
        /// User's last name
        /// </summary>
        [Required]
        [StringLength(100)]
        public string Surname { get; init; } = string.Empty;

        /// <summary>
        /// Email address
        /// </summary>
        [Required]
        [EmailAddress]
        public string Email { get; init; } = string.Empty;

        /// <summary>
        /// Password for the new account
        /// </summary>
        [Required]
        [StringLength(100, MinimumLength = 6)]
        public string Password { get; init; } = string.Empty;

        /// <summary>
        /// Phone number (optional)
        /// </summary>
        [Phone]
        public string? PhoneNumber { get; init; }
    }
}
