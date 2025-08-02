using Microsoft.AspNetCore.DataProtection;

namespace ShipMvp.Core.Security;

/// <summary>
/// Data Protection API-based encryption service implementation
/// </summary>
public class DataProtectionEncryptionService : IEncryptionService
{
    private readonly IDataProtector _protector;
    private const string Purpose = "IntegrationCredentialTokens";

    public DataProtectionEncryptionService(IDataProtectionProvider dataProtectionProvider)
    {
        _protector = dataProtectionProvider.CreateProtector(Purpose);
    }

    public string Encrypt(string plainText)
    {
        if (string.IsNullOrEmpty(plainText))
            return plainText;

        try
        {
            var encryptedBytes = _protector.Protect(System.Text.Encoding.UTF8.GetBytes(plainText));
            return Convert.ToBase64String(encryptedBytes);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException("Failed to encrypt text", ex);
        }
    }

    public string Decrypt(string encryptedText)
    {
        if (string.IsNullOrEmpty(encryptedText) || !IsEncrypted(encryptedText))
            return encryptedText;

        try
        {
            var encryptedBytes = Convert.FromBase64String(encryptedText);
            var decryptedBytes = _protector.Unprotect(encryptedBytes);
            return System.Text.Encoding.UTF8.GetString(decryptedBytes);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException("Failed to decrypt text", ex);
        }
    }

    public bool IsEncrypted(string text)
    {
        if (string.IsNullOrEmpty(text))
            return false;

        try
        {
            // Check if it's a valid base64 string
            var bytes = Convert.FromBase64String(text);
            // Additional check: encrypted data should be longer than original due to overhead
            return bytes.Length > 0;
        }
        catch
        {
            return false;
        }
    }
} 