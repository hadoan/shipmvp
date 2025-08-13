using ShipMvp.Domain.Integrations;

namespace ShipMvp.Application.Integrations;

public interface IIntegrationAppService
{
    Task<IntegrationDto> CreateAsync(CreateIntegrationDto createDto, CancellationToken cancellationToken = default);
    Task<IntegrationDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IEnumerable<IntegrationListDto>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<IEnumerable<IntegrationDto>> GetByIntegrationTypeAsync(IntegrationType integrationType, CancellationToken cancellationToken = default);
    Task<IntegrationDto?> UpdateAsync(Guid id, UpdateIntegrationDto updateDto, CancellationToken cancellationToken = default);
    Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default);
    Task<bool> ExistsWithNameAsync(string name, Guid? excludeId = null, CancellationToken cancellationToken = default);
    Task<IntegrationDto?> GetByPlatformTypeAsync(string platform, CancellationToken cancellationToken = default);
}