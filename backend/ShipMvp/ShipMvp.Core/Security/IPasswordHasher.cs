namespace ShipMvp.Core.Security;

/// <summary>
/// Service for hashing and verifying passwords
/// </summary>
public interface IPasswordHasher
{
    /// <summary>
    /// Hash a plain text password
    /// </summary>
    /// <param name="password">Plain text password</param>
    /// <returns>Hashed password</returns>
    string HashPassword(string password);
    
    /// <summary>
    /// Verify a plain text password against a hash
    /// </summary>
    /// <param name="hashedPassword">Hashed password</param>
    /// <param name="providedPassword">Plain text password to verify</param>
    /// <returns>True if password matches</returns>
    bool VerifyPassword(string hashedPassword, string providedPassword);
}
