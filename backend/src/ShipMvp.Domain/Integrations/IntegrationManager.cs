using ShipMvp.Domain.Integrations;
using ShipMvp.Core.Security;
using Microsoft.Extensions.Logging;

namespace ShipMvp.Domain.Integrations;

public interface IIntegrationManager
{
    Task<Integration?> GetByNameAsync(string name, CancellationToken cancellationToken = default);
    Task<Integration?> GetByTypeAsync(string type, CancellationToken cancellationToken = default);
    Task<IEnumerable<Integration>> GetByIntegrationTypeAsync(IntegrationType type, CancellationToken cancellationToken = default);
    Task<IntegrationCredential> CreateOrUpdateGenericCredentialAsync(
        Guid userId, 
        Guid integrationId, 
        string userInfo, 
        Dictionary<string, string> credentials,
        CancellationToken cancellationToken = default);
}

public class IntegrationManager : IIntegrationManager
{
    private readonly IIntegrationRepository _repository;
    private readonly IEncryptionService _encryptionService;
    private readonly ILogger<IntegrationManager> _logger;

    public IntegrationManager(IIntegrationRepository repository, IEncryptionService encryptionService, ILogger<IntegrationManager> logger)
    {
        _repository = repository;
        _encryptionService = encryptionService;
        _logger = logger;
    }

    public Task<Integration?> GetByNameAsync(string name, CancellationToken cancellationToken = default)
    {
        return _repository.GetByNameAsync(name, cancellationToken);
    }

    public Task<Integration?> GetByTypeAsync(string type, CancellationToken cancellationToken = default)
    {
        return _repository.GetByTypeAsync(type, cancellationToken);
    }

    public Task<IEnumerable<Integration>> GetByIntegrationTypeAsync(IntegrationType type, CancellationToken cancellationToken = default)
    {
        return _repository.GetByIntegrationTypeAsync(type, cancellationToken);
    }

    public async Task<IntegrationCredential> CreateOrUpdateGenericCredentialAsync(
        Guid userId, 
        Guid integrationId, 
        string userInfo, 
        Dictionary<string, string> credentials,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("IntegrationManager: Starting CreateOrUpdateGenericCredentialAsync for user {UserId}, integration {IntegrationId}, userInfo {UserInfo}", 
            userId, integrationId, userInfo);

        var existing = await _repository.GetCredentialByUserInfoAndIntegrationIdAsync(userInfo, integrationId, cancellationToken);
        if (existing != null)
        {
            _logger.LogInformation("IntegrationManager: Found existing credential with ID {CredentialId}, updating generic credentials", existing.Id);
            
            // Update each credential field
            foreach (var kvp in credentials)
            {
                var isEncrypted = ShouldEncryptField(kvp.Key);
                existing.SetCredentialField(kvp.Key, kvp.Value, isEncrypted);
            }
            
            // Encrypt sensitive fields
            await EncryptCredentialFieldsAsync(existing);
            
            _logger.LogInformation("IntegrationManager: Calling repository to update credential {CredentialId}", existing.Id);
            await _repository.UpdateCredentialAsync(existing, cancellationToken);
            _logger.LogInformation("IntegrationManager: Successfully updated credential {CredentialId}", existing.Id);
            return existing;
        }
        else
        {
            _logger.LogInformation("IntegrationManager: No existing credential found, creating new one");
            
            var newCredential = IntegrationCredential.Create(
                userId: userId,
                integrationId: integrationId,
                userInfo: userInfo
            );
            
            // Set each credential field
            foreach (var kvp in credentials)
            {
                var isEncrypted = ShouldEncryptField(kvp.Key);
                newCredential.SetCredentialField(kvp.Key, kvp.Value, isEncrypted);
            }
            
            _logger.LogInformation("IntegrationManager: Created new credential with ID {CredentialId}", newCredential.Id);
            
            // Encrypt sensitive fields
            await EncryptCredentialFieldsAsync(newCredential);
            
            _logger.LogInformation("IntegrationManager: Calling repository to add new credential {CredentialId}", newCredential.Id);
            await _repository.AddCredentialAsync(newCredential, cancellationToken);
            _logger.LogInformation("IntegrationManager: Successfully added new credential {CredentialId}", newCredential.Id);
            return newCredential;
        }
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

    private async Task EncryptCredentialFieldsAsync(IntegrationCredential credential)
    {
        foreach (var field in credential.CredentialFields.Where(f => f.IsEncrypted && !string.IsNullOrEmpty(f.Value)))
        {
            field.Value = _encryptionService.Encrypt(field.Value);
        }
    }
}