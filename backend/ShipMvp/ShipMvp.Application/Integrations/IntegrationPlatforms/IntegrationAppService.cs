using Microsoft.Extensions.Logging;
using ShipMvp.Core.Abstractions;
using ShipMvp.Core.Attributes;
using ShipMvp.Domain.Integrations;

namespace ShipMvp.Application.Integrations;

public class IntegrationAppService : IIntegrationAppService
{
    private readonly IIntegrationRepository _repository;
    private readonly IGuidGenerator _guidGenerator;
    private readonly ILogger<IntegrationAppService> _logger;

    public IntegrationAppService(
        IIntegrationRepository repository,
        IGuidGenerator guidGenerator,
        ILogger<IntegrationAppService> logger)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        _guidGenerator = guidGenerator ?? throw new ArgumentNullException(nameof(guidGenerator));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    [UnitOfWork]
    public async Task<IntegrationDto> CreateAsync(CreateIntegrationDto createDto, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Creating integration with name {Name}", createDto.Name);

        // Check if platform with same name already exists
        if (await _repository.ExistsWithNameAsync(createDto.Name, null, cancellationToken))
        {
            throw new InvalidOperationException($"Integration with name '{createDto.Name}' already exists");
        }

        var integration = Integration.Create(
            name: createDto.Name,
            createDto.IntegrationType,
            authMethod: createDto.AuthMethod,
            clientId: createDto.ClientId,
            clientSecret: createDto.ClientSecret,
            tokenEndpoint: createDto.TokenEndpoint,
            guidGenerator: _guidGenerator);

        var createdIntegration = await _repository.AddAsync(integration, cancellationToken);

        _logger.LogInformation("Created integration {IntegrationId} with name {Name}", createdIntegration.Id, createDto.Name);

        return MapToDto(createdIntegration);
    }

    public async Task<IntegrationDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var integration = await _repository.GetByIdAsync(id, cancellationToken);
        return integration != null ? MapToDto(integration) : null;
    }

    public async Task<IEnumerable<IntegrationListDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var integrations = await _repository.GetAllAsync(cancellationToken);
        return integrations.Select(MapToListDto);
    }

    public async Task<IEnumerable<IntegrationDto>> GetByIntegrationTypeAsync(IntegrationType integrationType, CancellationToken cancellationToken = default)
    {
        var integrations = await _repository.GetByIntegrationTypeAsync(integrationType, cancellationToken);
        return integrations.Select(MapToDto);
    }

    public async Task<IntegrationDto?> GetByPlatformTypeAsync(string platform, CancellationToken cancellationToken = default)
    {
        var integration = await _repository.GetByTypeAsync(platform, cancellationToken);
        return integration != null ? MapToDto(integration) : null;
    }

    [UnitOfWork]
    public async Task<IntegrationDto?> UpdateAsync(Guid id, UpdateIntegrationDto updateDto, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Updating integration {IntegrationId}", id);

        var existingIntegration = await _repository.GetByIdAsync(id, cancellationToken);
        if (existingIntegration == null)
        {
            _logger.LogWarning("Integration {IntegrationId} not found for update", id);
            return null;
        }

        // Check if name is being changed and if new name already exists
        if (!string.IsNullOrEmpty(updateDto.Name) && updateDto.Name != existingIntegration.Name)
        {
            if (await _repository.ExistsWithNameAsync(updateDto.Name, id, cancellationToken))
            {
                throw new InvalidOperationException($"Integration with name '{updateDto.Name}' already exists");
            }
        }

        existingIntegration.Update(
            name: updateDto.Name,
            clientId: updateDto.ClientId,
            clientSecret: updateDto.ClientSecret,
            tokenEndpoint: updateDto.TokenEndpoint);

        var result = await _repository.UpdateAsync(existingIntegration, cancellationToken);

        _logger.LogInformation("Updated integration {IntegrationId}", id);

        return MapToDto(result);
    }

    [UnitOfWork]
    public async Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Deleting integration {IntegrationId}", id);

        var integration = await _repository.GetByIdAsync(id, cancellationToken);
        if (integration == null)
        {
            _logger.LogWarning("Integration {IntegrationId} not found for deletion", id);
            return false;
        }

        await _repository.DeleteAsync(id, cancellationToken);

        _logger.LogInformation("Deleted integration {IntegrationId}", id);

        return true;
    }

    public async Task<bool> ExistsWithNameAsync(string name, Guid? excludeId = null, CancellationToken cancellationToken = default)
    {
        return await _repository.ExistsWithNameAsync(name, excludeId, cancellationToken);
    }

    private static IntegrationDto MapToDto(Integration integration)
    {
        return new IntegrationDto(
            Id: integration.Id,
            Name: integration.Name,
            IntegrationType: integration.Type,
            AuthMethod: integration.AuthMethod,
            ClientId: integration.ClientId,
            TokenEndpoint: integration.TokenEndpoint,
            CreatedAt: integration.CreatedAt,
            UpdatedAt: integration.UpdatedAt);
    }

    private static IntegrationListDto MapToListDto(Integration integration)
    {
        return new IntegrationListDto(
            Id: integration.Id,
            Name: integration.Name,
            IntegrationType: integration.Type,
            AuthMethod: integration.AuthMethod,
            CreatedAt: integration.CreatedAt);
    }
}