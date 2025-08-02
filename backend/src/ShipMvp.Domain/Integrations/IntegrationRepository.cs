using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ShipMvp.Core.Abstractions;
using ShipMvp.Core.Persistence;
using ShipMvp.Core.Attributes;
using Newtonsoft.Json;

namespace ShipMvp.Domain.Integrations;

[UnitOfWork]
public class IntegrationRepository : IIntegrationRepository
{
    private readonly IDbContext _context;
    private readonly DbSet<Integration> _integrationDbSet;
    private readonly DbSet<IntegrationCredential> _credentialDbSet;
    private readonly ILogger<IntegrationRepository> _logger;

    public IntegrationRepository(IDbContext context, ILogger<IntegrationRepository> logger)
    {
        _context = context;
        _logger = logger;
        _integrationDbSet = context.Set<Integration>();
        _credentialDbSet = context.Set<IntegrationCredential>();
    }

    // Integration platform methods
    public async Task<Integration?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _integrationDbSet.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
    }

    public async Task<IEnumerable<Integration>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _integrationDbSet.OrderBy(x => x.Name).ToListAsync(cancellationToken);
    }

    public async Task<Integration?> GetByNameAsync(string name, CancellationToken cancellationToken = default)
    {
        return await _integrationDbSet.FirstOrDefaultAsync(x => x.Name == name, cancellationToken);
    }

    public async Task<IEnumerable<Integration>> GetByIntegrationTypeAsync(IntegrationType integrationType, CancellationToken cancellationToken = default)
    {
        return await _integrationDbSet
            .Where(x => x.Type == integrationType)
            .OrderBy(x => x.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Integration>> GetByAuthMethodAsync(AuthMethodType authMethod, CancellationToken cancellationToken = default)
    {
        return await _integrationDbSet
            .Where(x => x.AuthMethod == authMethod)
            .OrderBy(x => x.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task<bool> ExistsWithNameAsync(string name, Guid? excludeId = null, CancellationToken cancellationToken = default)
    {
        var query = _integrationDbSet.Where(x => x.Name == name);
        if (excludeId.HasValue)
        {
            query = query.Where(x => x.Id != excludeId.Value);
        }
        return await query.AnyAsync(cancellationToken);
    }

    public async Task<Integration?> GetByTypeAsync(string type, CancellationToken cancellationToken = default)
    {
        if (!Enum.TryParse<IntegrationType>(type, out var integrationType))
            return null;
        return await _integrationDbSet.FirstOrDefaultAsync(x => x.Type == integrationType, cancellationToken);
    }

    [UnitOfWork]
    public async Task<Integration> AddAsync(Integration entity, CancellationToken cancellationToken = default)
    {
        Console.WriteLine($"Adding integration with name: {JsonConvert.SerializeObject(entity)}");
        await _integrationDbSet.AddAsync(entity, cancellationToken);
        var changesSaved = await _context.SaveChangesAsync(cancellationToken);
        Console.WriteLine($"Saved {changesSaved} changes to database");
        return entity;
    }

    public async Task<Integration> UpdateAsync(Integration entity, CancellationToken cancellationToken = default)
    {
        _integrationDbSet.Update(entity);
        await _context.SaveChangesAsync(cancellationToken);
        return entity;
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var entity = await GetByIdAsync(id, cancellationToken);
        if (entity != null)
        {
            _integrationDbSet.Remove(entity);
            await _context.SaveChangesAsync(cancellationToken);
        }
    }

    // Integration credential methods
    public async Task<IntegrationCredential?> GetCredentialByUserInfoAndIntegrationIdAsync(string userInfo, Guid integrationId, CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("IntegrationRepository: Looking for credential with userInfo {UserInfo} and integrationId {IntegrationId}", userInfo, integrationId);
        
        var credential = await _credentialDbSet
            .FirstOrDefaultAsync(x => x.UserInfo == userInfo && x.IntegrationId == integrationId, cancellationToken);
        
        if (credential != null)
        {
            _logger.LogDebug("IntegrationRepository: Found existing credential with ID {CredentialId}", credential.Id);
        }
        else
        {
            _logger.LogDebug("IntegrationRepository: No existing credential found for userInfo {UserInfo} and integrationId {IntegrationId}", userInfo, integrationId);
        }
        
        return credential;
    }

    public async Task UpdateCredentialAsync(IntegrationCredential credential, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("IntegrationRepository: Updating credential {CredentialId} for user {UserId} and integration {IntegrationId}", 
            credential.Id, credential.UserId, credential.IntegrationId);
        
        _credentialDbSet.Update(credential);
        var changesSaved = await _context.SaveChangesAsync(cancellationToken);
        
        _logger.LogInformation("IntegrationRepository: Successfully updated credential {CredentialId}. Changes saved: {ChangesSaved}", 
            credential.Id, changesSaved);
    }

    public async Task AddCredentialAsync(IntegrationCredential credential, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("IntegrationRepository: Adding new credential {CredentialId} for user {UserId} and integration {IntegrationId}", 
            credential.Id, credential.UserId, credential.IntegrationId);
        
        await _credentialDbSet.AddAsync(credential, cancellationToken);
        var changesSaved = await _context.SaveChangesAsync(cancellationToken);
        
        _logger.LogInformation("IntegrationRepository: Successfully added new credential {CredentialId}. Changes saved: {ChangesSaved}", 
            credential.Id, changesSaved);
    }

    public async Task<IntegrationCredential?> GetByUserAndIntegrationAsync(Guid userId, Guid integrationId, CancellationToken cancellationToken = default)
    {
        return await _credentialDbSet
            .FirstOrDefaultAsync(x => x.UserId == userId && x.IntegrationId == integrationId, cancellationToken);
    }

    public async Task<IEnumerable<IntegrationCredential>> GetByUserAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await _credentialDbSet
            .Where(x => x.UserId == userId)
            .OrderBy(x => x.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<IntegrationCredential>> GetByIntegrationAsync(Guid integrationId, CancellationToken cancellationToken = default)
    {
        return await _credentialDbSet
            .Where(x => x.IntegrationId == integrationId)
            .OrderBy(x => x.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<IntegrationCredential>> GetExpiredTokensAsync(CancellationToken cancellationToken = default)
    {
        var now = DateTime.UtcNow;
        var allCredentials = await _credentialDbSet
            .Include(x => x.CredentialFields)
            .ToListAsync(cancellationToken);
        
        return allCredentials
            .Where(x => x.CredentialFields.Any(cf => cf.Key == "expires_at" && 
                                                    DateTime.TryParse(cf.Value, out var expiresAt) && 
                                                    expiresAt <= now))
            .OrderBy(x => x.CreatedAt);
    }

    public async Task<bool> ExistsForUserAndIntegrationAsync(Guid userId, Guid integrationId, CancellationToken cancellationToken = default)
    {
        return await _credentialDbSet
            .AnyAsync(x => x.UserId == userId && x.IntegrationId == integrationId, cancellationToken);
    }

    public async Task<IntegrationCredential?> GetByUserAndPlatformAsync(Guid userId, string platform, CancellationToken cancellationToken = default)
    {
        // Join IntegrationCredentials with Integrations to match by platform (type)
        var integration = await _integrationDbSet
            .FirstOrDefaultAsync(x => x.Type.ToString() == platform, cancellationToken);
        if (integration == null)
            return null;
        return await _credentialDbSet
            .FirstOrDefaultAsync(x => x.UserId == userId && x.IntegrationId == integration.Id, cancellationToken);
    }

    public async Task DeleteCredentialAsync(Guid credentialId, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("IntegrationRepository: Deleting credential {CredentialId}", credentialId);
        
        var credential = await _credentialDbSet
            .Include(x => x.CredentialFields)
            .FirstOrDefaultAsync(x => x.Id == credentialId, cancellationToken);
        
        if (credential != null)
        {
            // Remove credential fields first (due to foreign key constraints)
            _context.Set<CredentialField>().RemoveRange(credential.CredentialFields);
            
            // Remove the credential
            _credentialDbSet.Remove(credential);
            
            var changesSaved = await _context.SaveChangesAsync(cancellationToken);
            _logger.LogInformation("IntegrationRepository: Successfully deleted credential {CredentialId}. Changes saved: {ChangesSaved}", 
                credentialId, changesSaved);
        }
        else
        {
            _logger.LogWarning("IntegrationRepository: Credential {CredentialId} not found for deletion", credentialId);
        }
    }
}