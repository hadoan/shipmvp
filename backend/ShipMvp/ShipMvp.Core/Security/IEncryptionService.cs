namespace ShipMvp.Core.Security;

/// <summary>
/// Service for encrypting and decrypting sensitive data
/// </summary>
public interface IEncryptionService
{
    /// <summary>
    /// Encrypt a plain text string
    /// </summary>
    /// <param name="plainText">Text to encrypt</param>
    /// <returns>Encrypted text</returns>
    string Encrypt(string plainText);
    
    /// <summary>
    /// Decrypt an encrypted string
    /// </summary>
    /// <param name="encryptedText">Text to decrypt</param>
    /// <returns>Decrypted text</returns>
    string Decrypt(string encryptedText);
    
    /// <summary>
    /// Check if a string is encrypted
    /// </summary>
    /// <param name="text">Text to check</param>
    /// <returns>True if the text appears to be encrypted</returns>
    bool IsEncrypted(string text);
} 