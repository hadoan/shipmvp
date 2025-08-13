using ShipMvp.Domain.Identity;
using ShipMvp.Core;
using ShipMvp.Core.Security;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using ShipMvp.Core.Persistence;
using Microsoft.Extensions.Logging;
using ShipMvp.Core.Abstractions;
using ShipMvp.Domain.Shared;

namespace ShipMvp.Application.Identity;

// Application Service Interface for JWT token generation
public interface IJwtTokenService : IScopedService
{
    string GenerateToken(Guid userId, string username, IList<string> roles);
    ClaimsPrincipal? ValidateToken(string token);
}

// Application DTOs
public record UserDto
{
    public Guid Id { get; init; }
    public string Username { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
    public string Surname { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
    public string? PhoneNumber { get; init; }
    public bool IsActive { get; init; }
    public bool IsEmailConfirmed { get; init; }
    public bool IsPhoneNumberConfirmed { get; init; }
    public bool IsLockoutEnabled { get; init; }
    public DateTime CreatedAt { get; init; }
    public DateTime? LastLoginAt { get; init; }
    public List<string> Roles { get; init; } = new();
}

public record CreateUserDto
{
    [Required]
    [StringLength(50, MinimumLength = 3)]
    public string Username { get; init; } = string.Empty;

    [Required]
    [StringLength(100)]
    public string Name { get; init; } = string.Empty;

    [Required]
    [StringLength(100)]
    public string Surname { get; init; } = string.Empty;

    [Required]
    [EmailAddress]
    public string Email { get; init; } = string.Empty;

    [Required]
    [StringLength(100, MinimumLength = 6)]
    public string Password { get; init; } = string.Empty;

    [Phone]
    public string? PhoneNumber { get; init; }

    public bool IsActive { get; init; } = true;
}

public record UpdateUserDto
{
    [Required]
    [StringLength(100)]
    public string Name { get; init; } = string.Empty;

    [Required]
    [StringLength(100)]
    public string Surname { get; init; } = string.Empty;

    [Phone]
    public string? PhoneNumber { get; init; }

    public bool IsActive { get; init; }
    public bool IsEmailConfirmed { get; init; }
    public bool IsPhoneNumberConfirmed { get; init; }
}

public record LoginDto
{
    [Required]
    [EmailAddress]
    public string Email { get; init; } = string.Empty;

    [Required]
    public string Password { get; init; } = string.Empty;

    public bool RememberMe { get; init; }
}

public record AuthResultDto
{
    public bool Success { get; init; }
    public string? Token { get; init; }
    public UserDto? User { get; init; }
    public string? ErrorMessage { get; init; }
}

public record GetUsersQuery
{
    public string? SearchText { get; init; }
    public string? Role { get; init; }
    public bool? IsActive { get; init; }
    public int PageSize { get; init; } = 10;
    public int PageNumber { get; init; } = 1;
}

// Application Service Interfaces
public interface IUserService : IScopedService
{
    Task<UserDto> CreateAsync(CreateUserDto request, CancellationToken cancellationToken = default);
    Task<UserDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<UserDto?> GetByUsernameAsync(string username, CancellationToken cancellationToken = default);
    Task<IEnumerable<UserDto>> GetListAsync(GetUsersQuery query, CancellationToken cancellationToken = default);
    Task<UserDto> UpdateAsync(Guid id, UpdateUserDto request, CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
    Task<UserDto> AddToRoleAsync(Guid id, string role, CancellationToken cancellationToken = default);
    Task<UserDto> RemoveFromRoleAsync(Guid id, string role, CancellationToken cancellationToken = default);
}

public interface IAuthService : IScopedService
{
    Task<AuthResultDto> LoginAsync(LoginDto request, CancellationToken cancellationToken = default);
    Task LogoutAsync(CancellationToken cancellationToken = default);
    Task<AuthResultDto> RefreshTokenAsync(string token, CancellationToken cancellationToken = default);
}

// Application Service Implementation
public class UserService : IUserService
{
    private readonly IUserRepository _repository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IPasswordHasher _passwordHasher;

    public UserService(IUserRepository repository, IUnitOfWork unitOfWork, IPasswordHasher passwordHasher)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
        _passwordHasher = passwordHasher;
    }

    public async Task<UserDto> CreateAsync(CreateUserDto request, CancellationToken cancellationToken = default)
    {
        // Check uniqueness
        if (!await _repository.IsUsernameUniqueAsync(request.Username, cancellationToken: cancellationToken))
            throw new InvalidOperationException($"Username '{request.Username}' is already taken");

        if (!await _repository.IsEmailUniqueAsync(request.Email, cancellationToken: cancellationToken))
            throw new InvalidOperationException($"Email '{request.Email}' is already taken");

        var email = request.Email.ToLowerInvariant();
        var phoneNumber = PhoneNumber.CreateOrDefault(request.PhoneNumber);
        var passwordHash = _passwordHasher.HashPassword(request.Password);

        var user = new User(
            Guid.NewGuid(),
            request.Username,
            request.Name,
            request.Surname,
            email,
            passwordHash,
            phoneNumber,
            request.IsActive
        );

        await _repository.AddAsync(user, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return MapToDto(user);
    }

    public async Task<UserDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var user = await _repository.GetByIdAsync(id, cancellationToken);
        return user != null ? MapToDto(user) : null;
    }

    public async Task<UserDto?> GetByUsernameAsync(string username, CancellationToken cancellationToken = default)
    {
        var user = await _repository.GetByUsernameAsync(username, cancellationToken);
        return user != null ? MapToDto(user) : null;
    }

    public async Task<IEnumerable<UserDto>> GetListAsync(GetUsersQuery query, CancellationToken cancellationToken = default)
    {
        IEnumerable<User> users;

        if (!string.IsNullOrEmpty(query.Role))
        {
            users = await _repository.GetByRoleAsync(query.Role, cancellationToken);
        }
        else
        {
            users = await _repository.GetAllAsync(cancellationToken);
        }

        // Apply filters
        if (!string.IsNullOrEmpty(query.SearchText))
        {
            var searchLower = query.SearchText.ToLower();
            users = users.Where(u =>
                u.Username.ToLower().Contains(searchLower) ||
                u.Name.ToLower().Contains(searchLower) ||
                u.Surname.ToLower().Contains(searchLower) ||
                u.Email.ToLower().Contains(searchLower));
        }

        if (query.IsActive.HasValue)
        {
            users = users.Where(u => u.IsActive == query.IsActive.Value);
        }

        return users.Select(MapToDto);
    }

    public async Task<UserDto> UpdateAsync(Guid id, UpdateUserDto request, CancellationToken cancellationToken = default)
    {
        var user = await _repository.GetByIdAsync(id, cancellationToken);
        if (user == null)
            throw new InvalidOperationException($"User {id} not found");

        var phoneNumber = PhoneNumber.CreateOrDefault(request.PhoneNumber);

        var updatedUser = user
            .UpdateInfo(request.Name, request.Surname, phoneNumber)
            .SetActive(request.IsActive);

        if (request.IsEmailConfirmed && !user.IsEmailConfirmed)
            updatedUser = updatedUser.ConfirmEmail();

        if (request.IsPhoneNumberConfirmed && !user.IsPhoneNumberConfirmed)
            updatedUser = updatedUser.ConfirmPhoneNumber();

        await _repository.UpdateAsync(updatedUser, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return MapToDto(updatedUser);
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        await _repository.DeleteAsync(id, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    public async Task<UserDto> AddToRoleAsync(Guid id, string role, CancellationToken cancellationToken = default)
    {
        var user = await _repository.GetByIdAsync(id, cancellationToken);
        if (user == null)
            throw new InvalidOperationException($"User {id} not found");

        var updatedUser = user.AddRole(role);
        await _repository.UpdateAsync(updatedUser, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return MapToDto(updatedUser);
    }

    public async Task<UserDto> RemoveFromRoleAsync(Guid id, string role, CancellationToken cancellationToken = default)
    {
        var user = await _repository.GetByIdAsync(id, cancellationToken);
        if (user == null)
            throw new InvalidOperationException($"User {id} not found");

        var updatedUser = user.RemoveRole(role);
        await _repository.UpdateAsync(updatedUser, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return MapToDto(updatedUser);
    }

    private static UserDto MapToDto(User user) => new()
    {
        Id = user.Id,
        Username = user.Username,
        Name = user.Name,
        Surname = user.Surname,
        Email = user.Email,
        PhoneNumber = user.PhoneNumber?.Value,
        IsActive = user.IsActive,
        IsEmailConfirmed = user.IsEmailConfirmed,
        IsPhoneNumberConfirmed = user.IsPhoneNumberConfirmed,
        IsLockoutEnabled = user.IsLockoutEnabled,
        CreatedAt = user.CreatedAt,
        LastLoginAt = user.LastLoginAt,
        Roles = user.Roles
    };
}

// Authentication Service with JWT token generation
public class AuthService : IAuthService
{
    private readonly IUserRepository _repository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IJwtTokenService _jwtTokenService;
    private readonly IPasswordHasher _passwordHasher;
    private readonly ILogger<AuthService> _logger;

    public AuthService(IUserRepository repository, IUnitOfWork unitOfWork, IJwtTokenService jwtTokenService, IPasswordHasher passwordHasher, ILogger<AuthService> logger)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
        _jwtTokenService = jwtTokenService;
        _passwordHasher = passwordHasher;
        _logger = logger;
    }

    public async Task<AuthResultDto> LoginAsync(LoginDto request, CancellationToken cancellationToken = default)
    {
        var normalizedEmail = request.Email.ToLowerInvariant();
        _logger.LogDebug("AuthService: Attempting login for email: {Email}", normalizedEmail);
        
        // Find user by email (normalized to lowercase)
        var user = await _repository.GetByEmailAsync(normalizedEmail, cancellationToken);
        _logger.LogDebug("AuthService: User found by email: {Found}", user != null);

        if (user == null || !user.IsActive)
        {
            _logger.LogWarning("AuthService: Login failed - user not found or inactive. Email: {Email}, User found: {Found}, IsActive: {IsActive}", 
                normalizedEmail, user != null, user?.IsActive);
            return new AuthResultDto
            {
                Success = false,
                ErrorMessage = "Invalid email or password"
            };
        }

        // Verify password
        if (!_passwordHasher.VerifyPassword(user.PasswordHash, request.Password))
        {
            _logger.LogWarning("AuthService: Login failed - invalid password for user: {Email}", normalizedEmail);
            return new AuthResultDto
            {
                Success = false,
                ErrorMessage = "Invalid email or password"
            };
        }

        _logger.LogDebug("AuthService: Password verified successfully for user: {Email}", normalizedEmail);

        var updatedUser = user.RecordLogin();
        await _repository.UpdateAsync(updatedUser, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // Generate JWT token using the token service
        var token = _jwtTokenService.GenerateToken(user.Id, user.Username, user.Roles);

        return new AuthResultDto
        {
            Success = true,
            Token = token,
            User = MapToDto(updatedUser)
        };
    }

    public Task LogoutAsync(CancellationToken cancellationToken = default)
    {
        // Implement logout logic (invalidate tokens, etc.)
        return Task.CompletedTask;
    }

    public Task<AuthResultDto> RefreshTokenAsync(string token, CancellationToken cancellationToken = default)
    {
        // Implement token refresh logic
        throw new NotImplementedException();
    }

    private static UserDto MapToDto(User user) => new()
    {
        Id = user.Id,
        Username = user.Username,
        Name = user.Name,
        Surname = user.Surname,
        Email = user.Email,
        PhoneNumber = user.PhoneNumber?.Value,
        IsActive = user.IsActive,
        IsEmailConfirmed = user.IsEmailConfirmed,
        IsPhoneNumberConfirmed = user.IsPhoneNumberConfirmed,
        IsLockoutEnabled = user.IsLockoutEnabled,
        CreatedAt = user.CreatedAt,
        LastLoginAt = user.LastLoginAt,
        Roles = user.Roles
    };
}
