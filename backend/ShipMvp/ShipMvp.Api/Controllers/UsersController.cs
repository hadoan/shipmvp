using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ShipMvp.Application.Identity;
using ShipMvp.Domain.Shared.Constants;
using Swashbuckle.AspNetCore.Annotations;

namespace ShipMvp.Api.Controllers;

/// <summary>
/// User management endpoints for CRUD operations
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
[Authorize]
public class UsersController : ControllerBase
{
    private readonly IUserService _userService;
    private readonly ILogger<UsersController> _logger;

    /// <summary>
    /// Initializes a new instance of the UsersController
    /// </summary>
    /// <param name="userService">User service for managing user operations</param>
    /// <param name="logger">Logger instance</param>
    public UsersController(IUserService userService, ILogger<UsersController> logger)
    {
        _userService = userService;
        _logger = logger;
    }

    /// <summary>
    /// Get a list of users with optional filtering
    /// </summary>
    /// <param name="searchText">Search text for username, name, surname, or email</param>
    /// <param name="role">Filter by role</param>
    /// <param name="isActive">Filter by active status</param>
    /// <param name="pageSize">Number of items per page (default: 10)</param>
    /// <param name="pageNumber">Page number (default: 1)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of users</returns>
    [HttpGet]
    [Authorize(Policy = Policies.RequireUserManagement)]
    [SwaggerOperation(
        Summary = "Get users",
        Description = "Retrieve a list of users with optional filtering and pagination"
    )]
    [SwaggerResponse(200, "Users retrieved successfully", typeof(IEnumerable<UserDto>))]
    public async Task<ActionResult<IEnumerable<UserDto>>> GetUsersAsync(
        [FromQuery] string? searchText = null,
        [FromQuery] string? role = null,
        [FromQuery] bool? isActive = null,
        [FromQuery] int pageSize = 10,
        [FromQuery] int pageNumber = 1,
        CancellationToken cancellationToken = default)
    {
        var query = new GetUsersQuery
        {
            SearchText = searchText,
            Role = role,
            IsActive = isActive,
            PageSize = pageSize,
            PageNumber = pageNumber
        };

        var users = await _userService.GetListAsync(query, cancellationToken);
        return Ok(users);
    }

    /// <summary>
    /// Get a user by ID
    /// </summary>
    /// <param name="id">User ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>User details</returns>
    [HttpGet("{id:guid}")]
    [Authorize(Policy = Policies.RequireReadOnly)]
    [SwaggerOperation(
        Summary = "Get user by ID",
        Description = "Retrieve a specific user by their ID"
    )]
    [SwaggerResponse(200, "User found", typeof(UserDto))]
    [SwaggerResponse(404, "User not found")]
    public async Task<ActionResult<UserDto>> GetUserAsync(
        [FromRoute] Guid id,
        CancellationToken cancellationToken = default)
    {
        var user = await _userService.GetByIdAsync(id, cancellationToken);

        if (user == null)
        {
            return NotFound($"User with ID {id} not found");
        }

        return Ok(user);
    }

    /// <summary>
    /// Create a new user
    /// </summary>
    /// <param name="request">User creation data</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Created user</returns>
    [HttpPost]
    [Authorize(Policy = Policies.RequireAdminRole)]
    [SwaggerOperation(
        Summary = "Create user",
        Description = "Create a new user in the system"
    )]
    [SwaggerResponse(201, "User created successfully", typeof(UserDto))]
    [SwaggerResponse(400, "Invalid user data")]
    [SwaggerResponse(409, "Username or email already exists")]
    public async Task<ActionResult<UserDto>> CreateUserAsync(
        [FromBody] CreateUserDto request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogDebug("Creating new user with username: {Username}", request.Username);
            var user = await _userService.CreateAsync(request, cancellationToken);
            _logger.LogInformation("User created successfully with ID: {UserId}", user.Id);
            return CreatedAtAction(nameof(GetUserAsync), new { id = user.Id }, user);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning("Failed to create user with username: {Username}. Reason: {Reason}", request.Username, ex.Message);
            return Conflict(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating user with username: {Username}", request.Username);
            return BadRequest(new { message = "Failed to create user" });
        }
    }

    /// <summary>
    /// Update an existing user
    /// </summary>
    /// <param name="id">User ID</param>
    /// <param name="request">User update data</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Updated user</returns>
    [HttpPut("{id:guid}")]
    [Authorize(Policy = Policies.RequireUserManagement)]
    [SwaggerOperation(
        Summary = "Update user",
        Description = "Update an existing user's information"
    )]
    [SwaggerResponse(200, "User updated successfully", typeof(UserDto))]
    [SwaggerResponse(404, "User not found")]
    [SwaggerResponse(400, "Invalid user data")]
    public async Task<ActionResult<UserDto>> UpdateUserAsync(
        [FromRoute] Guid id,
        [FromBody] UpdateUserDto request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogDebug("Updating user: {UserId}", id);
            var user = await _userService.UpdateAsync(id, request, cancellationToken);
            _logger.LogInformation("User updated successfully: {UserId}", id);
            return Ok(user);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning("Failed to update user: {UserId}. Reason: {Reason}", id, ex.Message);
            return NotFound(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating user: {UserId}", id);
            return BadRequest(new { message = "Failed to update user" });
        }
    }

    /// <summary>
    /// Delete a user
    /// </summary>
    /// <param name="id">User ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Confirmation of deletion</returns>
    [HttpDelete("{id:guid}")]
    [Authorize(Policy = Policies.RequireAdminRole)]
    [SwaggerOperation(
        Summary = "Delete user",
        Description = "Delete a user from the system"
    )]
    [SwaggerResponse(204, "User deleted successfully")]
    [SwaggerResponse(404, "User not found")]
    public async Task<ActionResult> DeleteUserAsync(
        [FromRoute] Guid id,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogDebug("Deleting user: {UserId}", id);
            await _userService.DeleteAsync(id, cancellationToken);
            _logger.LogInformation("User deleted successfully: {UserId}", id);
            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting user: {UserId}", id);
            return NotFound(new { message = $"User with ID {id} not found" });
        }
    }

    /// <summary>
    /// Add a role to a user
    /// </summary>
    /// <param name="id">User ID</param>
    /// <param name="request">Role assignment request</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Updated user with new role</returns>
    [HttpPost("{id:guid}/roles")]
    [Authorize(Policy = Policies.RequireAdminRole)]
    [SwaggerOperation(
        Summary = "Add role to user",
        Description = "Assign a role to a user"
    )]
    [SwaggerResponse(200, "Role added successfully", typeof(UserDto))]
    [SwaggerResponse(404, "User not found")]
    public async Task<ActionResult<UserDto>> AddRoleAsync(
        [FromRoute] Guid id,
        [FromBody] RoleRequest request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogDebug("Adding role {Role} to user: {UserId}", request.Role, id);
            var user = await _userService.AddToRoleAsync(id, request.Role, cancellationToken);
            _logger.LogInformation("Role {Role} added successfully to user: {UserId}", request.Role, id);
            return Ok(user);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning("Failed to add role {Role} to user: {UserId}. Reason: {Reason}", request.Role, id, ex.Message);
            return NotFound(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding role {Role} to user: {UserId}", request.Role, id);
            return BadRequest(new { message = "Failed to add role to user" });
        }
    }

    /// <summary>
    /// Remove a role from a user
    /// </summary>
    /// <param name="id">User ID</param>
    /// <param name="role">Role name to remove</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Updated user without the role</returns>
    [HttpDelete("{id:guid}/roles/{role}")]
    [Authorize(Policy = Policies.RequireAdminRole)]
    [SwaggerOperation(
        Summary = "Remove role from user",
        Description = "Remove a role from a user"
    )]
    [SwaggerResponse(200, "Role removed successfully", typeof(UserDto))]
    [SwaggerResponse(404, "User not found")]
    public async Task<ActionResult<UserDto>> RemoveRoleAsync(
        [FromRoute] Guid id,
        [FromRoute] string role,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogDebug("Removing role {Role} from user: {UserId}", role, id);
            var user = await _userService.RemoveFromRoleAsync(id, role, cancellationToken);
            _logger.LogInformation("Role {Role} removed successfully from user: {UserId}", role, id);
            return Ok(user);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning("Failed to remove role {Role} from user: {UserId}. Reason: {Reason}", role, id, ex.Message);
            return NotFound(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error removing role {Role} from user: {UserId}", role, id);
            return BadRequest(new { message = "Failed to remove role from user" });
        }
    }
}

/// <summary>
/// Request model for role operations
/// </summary>
public record RoleRequest
{
    /// <summary>
    /// The role name to assign or remove
    /// </summary>
    public string Role { get; init; } = string.Empty;
}
