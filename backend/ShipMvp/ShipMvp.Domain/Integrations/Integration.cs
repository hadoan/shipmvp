using ShipMvp.Core;
using ShipMvp.Core.Abstractions;
using ShipMvp.Core.Entities;
using ShipMvp.Domain.Integrations.Constants;

namespace ShipMvp.Domain.Integrations;

public sealed class Integration : AggregateRoot<Guid>
{
    public string Name { get; set; }
    public IntegrationType Type { get; set; }
    public AuthMethodType AuthMethod { get; set; }
    public string? ClientId { get; set; }
    public string? ClientSecret { get; set; }
    public string? TokenEndpoint { get; set; }

    // Parameterless constructor for EF Core
    private Integration() : base(Guid.Empty)
    {
        Name = string.Empty;
    }

    public Integration(
        Guid id,
        string name,
        IntegrationType type,
        AuthMethodType authMethod,
        // ...existing code...
        string? clientId = null,
        string? clientSecret = null,
        string? tokenEndpoint = null)
        : base(id)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Name is required", nameof(name));
        if (name.Length > IntegrationConsts.Integration.NameMaxLength)
            throw new ArgumentException($"Name cannot exceed {IntegrationConsts.Integration.NameMaxLength} characters", nameof(name));

        Name = name;
        Type = type;
        AuthMethod = authMethod;
        ClientId = clientId;
        ClientSecret = clientSecret;
        TokenEndpoint = tokenEndpoint;
    }

    public static Integration Create(
        string name,
        IntegrationType type,
        AuthMethodType authMethod,
        // ...existing code...
        string? clientId = null,
        string? clientSecret = null,
        string? tokenEndpoint = null,
        IGuidGenerator? guidGenerator = null)
    {
        var id = guidGenerator?.Create() ?? Guid.NewGuid();
        return new Integration(id, name, type, authMethod, clientId, clientSecret, tokenEndpoint);
    }

    public void Update(
        string? name = null,
        // ...existing code...
        string? clientId = null,
        string? clientSecret = null,
        string? tokenEndpoint = null)
    {
        if (name != null) Name = name;
        if (clientId != null) ClientId = clientId;
        if (clientSecret != null) ClientSecret = clientSecret;
        if (tokenEndpoint != null) TokenEndpoint = tokenEndpoint;
    }

    // Navigation property for EF Core - removed to prevent shadow property conflicts
    // public ICollection<IntegrationCredential> IntegrationCredentials { get; set; } = new List<IntegrationCredential>();
} 