using ShipMvp.Core;
using ShipMvp.Core.Entities;
using ShipMvp.Core.Abstractions;

namespace ShipMvp.Domain.Integrations;

public class CredentialField : Entity<Guid>
{
    public Guid IntegrationCredentialId { get; set; }
    public string Key { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
    public bool IsEncrypted { get; set; }
    public new DateTime CreatedAt { get; set; }
    public new DateTime? UpdatedAt { get; set; }

    // Navigation property
    public IntegrationCredential IntegrationCredential { get; set; } = null!;

    // Parameterless constructor for EF Core
    private CredentialField() : base(Guid.Empty)
    {
    }

    public CredentialField(
        Guid id,
        Guid integrationCredentialId,
        string key,
        string value,
        bool isEncrypted = true)
        : base(id)
    {
        IntegrationCredentialId = integrationCredentialId;
        Key = key ?? throw new ArgumentNullException(nameof(key));
        Value = value ?? throw new ArgumentNullException(nameof(value));
        IsEncrypted = isEncrypted;
        CreatedAt = DateTime.UtcNow;
    }

    public static CredentialField Create(
        Guid integrationCredentialId,
        string key,
        string value,
        bool isEncrypted = true,
        IGuidGenerator? guidGenerator = null)
    {
        var id = guidGenerator?.Create() ?? Guid.NewGuid();
        return new CredentialField(id, integrationCredentialId, key, value, isEncrypted);
    }

    public void UpdateValue(string value)
    {
        Value = value ?? throw new ArgumentNullException(nameof(value));
        UpdatedAt = DateTime.UtcNow;
    }
} 