using ShipMvp.Core;
using ShipMvp.Core.Abstractions;
using ShipMvp.Core.Entities;

namespace ShipMvp.Domain.Integrations;

public sealed class IntegrationCredential : Entity<Guid>
{
    // Core fields
    public string UserInfo { get; set; } = string.Empty;
    public Guid IntegrationId { get; set; }
    public Guid UserId { get; set; }
    
    // Generic credential storage
    public ICollection<CredentialField> CredentialFields { get; set; } = new List<CredentialField>();
    
    // Audit fields
    public new DateTime CreatedAt { get; set; }
    public new DateTime? UpdatedAt { get; set; }
    public new DateTime? DeletedAt { get; set; }
    public new bool IsDeleted { get; set; }

    // Parameterless constructor for EF Core
    private IntegrationCredential() : base(Guid.Empty)
    {
    }

    public IntegrationCredential(
        Guid id,
        Guid userId,
        Guid integrationId,
        string userInfo)
        : base(id)
    {
        UserId = userId;
        IntegrationId = integrationId;
        UserInfo = userInfo ?? throw new ArgumentNullException(nameof(userInfo));
        CreatedAt = DateTime.UtcNow;
        IsDeleted = false;
    }

    public static IntegrationCredential Create(
        Guid userId,
        Guid integrationId,
        string userInfo,
        IGuidGenerator? guidGenerator = null)
    {
        var id = guidGenerator?.Create() ?? Guid.NewGuid();
        return new IntegrationCredential(id, userId, integrationId, userInfo);
    }

    // Generic credential methods
    public void SetCredentialField(string key, string value, bool isEncrypted = true)
    {
        var existingField = CredentialFields.FirstOrDefault(f => f.Key == key);
        if (existingField != null)
        {
            existingField.UpdateValue(value);
        }
        else
        {
            CredentialFields.Add(CredentialField.Create(Id, key, value, isEncrypted));
        }
        UpdatedAt = DateTime.UtcNow;
    }

    public string? GetCredentialField(string key)
    {
        return CredentialFields.FirstOrDefault(f => f.Key == key)?.Value;
    }

    public bool HasCredentialField(string key)
    {
        return CredentialFields.Any(f => f.Key == key);
    }

    public void RemoveCredentialField(string key)
    {
        var field = CredentialFields.FirstOrDefault(f => f.Key == key);
        if (field != null)
        {
            CredentialFields.Remove(field);
            UpdatedAt = DateTime.UtcNow;
        }
    }

    public void UpdateCredentialFields(Dictionary<string, string> credentials, bool encryptSensitive = true)
    {
        foreach (var kvp in credentials)
        {
            var isEncrypted = encryptSensitive && ShouldEncryptField(kvp.Key);
            SetCredentialField(kvp.Key, kvp.Value, isEncrypted);
        }
        UpdatedAt = DateTime.UtcNow;
    }

    private bool ShouldEncryptField(string fieldKey)
    {
        // Define which fields should be encrypted
        var encryptedFields = new[]
        {
            "access_token", "refresh_token", "api_key", "client_secret", 
            "deployment", "endpoint", "organization", "api_secret"
        };
        
        return encryptedFields.Contains(fieldKey.ToLowerInvariant());
    }

    public IntegrationCredential MarkAsDeleted()
    {
        IsDeleted = true;
        DeletedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
        return this;
    }

    // Navigation property for EF Core
    public Integration Integration { get; set; } = null!;
}

