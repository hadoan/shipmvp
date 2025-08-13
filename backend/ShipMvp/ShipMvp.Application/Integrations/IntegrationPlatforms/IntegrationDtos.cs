using ShipMvp.Domain.Integrations;

namespace ShipMvp.Application.Integrations;

// DTOs for Integration operations
// Note: IntegrationType in the DTO maps to Type in the domain model.
public record CreateIntegrationDto(
    string Name,
    IntegrationType IntegrationType,
    AuthMethodType AuthMethod,
    string? ClientId = null,
    string? ClientSecret = null,
    string? TokenEndpoint = null);

public record UpdateIntegrationDto(
    string? Name = null,
    string? ClientId = null,
    string? ClientSecret = null,
    string? TokenEndpoint = null);

public record IntegrationDto(
    Guid Id,
    string Name,
    IntegrationType IntegrationType,
    AuthMethodType AuthMethod,
    string? ClientId,
    string? TokenEndpoint,
    DateTime CreatedAt,
    DateTime? UpdatedAt);

public record IntegrationListDto(
    Guid Id,
    string Name,
    IntegrationType IntegrationType,
    AuthMethodType AuthMethod,
    DateTime CreatedAt); 