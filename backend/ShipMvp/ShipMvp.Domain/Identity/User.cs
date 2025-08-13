using ShipMvp.Core;
using ShipMvp.Core.Abstractions;
using ShipMvp.Core.Entities;

namespace ShipMvp.Domain.Identity;

public sealed record PhoneNumber(string Value)
{
    // Parameterless constructor for EF Core
    private PhoneNumber() : this(string.Empty) { }
    
    public static PhoneNumber? CreateOrDefault(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return null;
        return new PhoneNumber(value.Trim());
    }
}

// Domain Entity
public sealed class User : Core.Entities.AggregateRoot<Guid>
{
    public string Username { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Surname { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public PhoneNumber? PhoneNumber { get; set; }
    public bool IsActive { get; set; }
    public bool IsEmailConfirmed { get; set; }
    public bool IsPhoneNumberConfirmed { get; set; }
    public bool IsLockoutEnabled { get; set; }
    public DateTime? LastLoginAt { get; set; }
    public List<string> Roles { get; set; } = new();
    public string PasswordHash { get; set; } = string.Empty;

    // Parameterless constructor for EF Core
    private User() : base(Guid.Empty) { }

    public User(Guid id, string username, string name, string surname, string email, 
                string passwordHash, PhoneNumber? phoneNumber = null, bool isActive = true) 
        : base(id)
    {
        if (string.IsNullOrWhiteSpace(email))
            throw new ArgumentException("Email is required", nameof(email));
            
        Username = username;
        Name = name;
        Surname = surname;
        Email = email.ToLowerInvariant();
        PasswordHash = passwordHash;
        PhoneNumber = phoneNumber;
        IsActive = isActive;
        IsEmailConfirmed = false;
        IsPhoneNumberConfirmed = false;
        IsLockoutEnabled = true;
        Roles = new List<string>();
    }
    
    public User UpdateInfo(string name, string surname, PhoneNumber? phoneNumber)
    {
        Name = name;
        Surname = surname;
        PhoneNumber = phoneNumber;
        return this;
    }
    
    public User SetActive(bool isActive)
    {
        IsActive = isActive;
        return this;
    }
    
    public User ConfirmEmail()
    {
        IsEmailConfirmed = true;
        return this;
    }
    
    public User ConfirmPhoneNumber()
    {
        IsPhoneNumberConfirmed = true;
        return this;
    }
    
    public User RecordLogin()
    {
        LastLoginAt = DateTime.UtcNow;
        return this;
    }
    
    public User AddRole(string role)
    {
        if (!Roles.Contains(role))
        {
            Roles.Add(role);
        }
        return this;
    }
    
    public User RemoveRole(string role)
    {
        if (Roles.Contains(role))
        {
            Roles = Roles.Where(r => r != role).ToList();
        }
        return this;
    }
    
    public User UpdatePassword(string passwordHash)
    {
        PasswordHash = passwordHash;
        return this;
    }
    
    public bool VerifyPassword(string passwordHash)
    {
        return PasswordHash == passwordHash;
    }
}

// Domain Service Interface
public interface IUserRepository : IRepository<User, Guid>
{
    Task<User?> GetByUsernameAsync(string username, CancellationToken cancellationToken = default);
    Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default);
    Task<IEnumerable<User>> GetByRoleAsync(string role, CancellationToken cancellationToken = default);
    Task<bool> IsUsernameUniqueAsync(string username, Guid? excludeUserId = null, CancellationToken cancellationToken = default);
    Task<bool> IsEmailUniqueAsync(string email, Guid? excludeUserId = null, CancellationToken cancellationToken = default);
}
