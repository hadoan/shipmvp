using System.Security.Cryptography;
using System.Text;

namespace ShipMvp.Core.Security;

/// <summary>
/// BCrypt-based password hasher implementation
/// </summary>
public class PasswordHasher : IPasswordHasher
{
    private const int WorkFactor = 12;
    
    public string HashPassword(string password)
    {
        if (string.IsNullOrEmpty(password))
            throw new ArgumentException("Password cannot be null or empty", nameof(password));
            
        return BCrypt.Net.BCrypt.HashPassword(password, WorkFactor);
    }
    
    public bool VerifyPassword(string hashedPassword, string providedPassword)
    {
        if (string.IsNullOrEmpty(hashedPassword))
            throw new ArgumentException("Hashed password cannot be null or empty", nameof(hashedPassword));
            
        if (string.IsNullOrEmpty(providedPassword))
            return false;
            
        try
        {
            return BCrypt.Net.BCrypt.Verify(providedPassword, hashedPassword);
        }
        catch
        {
            return false;
        }
    }
}
