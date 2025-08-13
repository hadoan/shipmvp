using ShipMvp.Core.Abstractions;

namespace ShipMvp.Domain.Integrations;

/// <summary>
/// Repository interface for managing integrations and integration credentials
/// </summary>
public interface IIntegrationRepository : IRepository<Integration, Guid>
{
    // Integration platform methods
    Task<Integration?> GetByNameAsync(string name, CancellationToken cancellationToken = default);
    Task<IEnumerable<Integration>> GetByIntegrationTypeAsync(IntegrationType integrationType, CancellationToken cancellationToken = default);
    Task<IEnumerable<Integration>> GetByAuthMethodAsync(AuthMethodType authMethod, CancellationToken cancellationToken = default);
    Task<bool> ExistsWithNameAsync(string name, Guid? excludeId = null, CancellationToken cancellationToken = default);
    Task<Integration?> GetByTypeAsync(string type, CancellationToken cancellationToken = default);
    
    // Integration credential methods
    Task<IntegrationCredential?> GetCredentialByUserInfoAndIntegrationIdAsync(string userInfo, Guid integrationId, CancellationToken cancellationToken = default);
    Task UpdateCredentialAsync(IntegrationCredential credential, CancellationToken cancellationToken = default);
    Task AddCredentialAsync(IntegrationCredential credential, CancellationToken cancellationToken = default);
    Task<IntegrationCredential?> GetByUserAndIntegrationAsync(Guid userId, Guid integrationId, CancellationToken cancellationToken = default);
    Task<IEnumerable<IntegrationCredential>> GetByUserAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<IEnumerable<IntegrationCredential>> GetByIntegrationAsync(Guid integrationId, CancellationToken cancellationToken = default);
    Task<IEnumerable<IntegrationCredential>> GetExpiredTokensAsync(CancellationToken cancellationToken = default);
    Task<bool> ExistsForUserAndIntegrationAsync(Guid userId, Guid integrationId, CancellationToken cancellationToken = default);
    Task<IntegrationCredential?> GetByUserAndPlatformAsync(Guid userId, string platform, CancellationToken cancellationToken = default);
    Task DeleteCredentialAsync(Guid credentialId, CancellationToken cancellationToken = default);
}