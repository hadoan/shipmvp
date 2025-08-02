using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using ShipMvp.Application.Integrations;
using ShipMvp.Domain.Integrations;
using Swashbuckle.AspNetCore.Annotations;

namespace ShipMvp.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[SwaggerTag("Integration management")]
public class IntegrationsController : ControllerBase
{
    private readonly IIntegrationAppService _integrationAppService;
    private readonly IIntegrationManager _integrationManager;
    private readonly ILogger<IntegrationsController> _logger;

    public IntegrationsController(
        IIntegrationAppService integrationAppService,
        IIntegrationManager integrationManager,
        ILogger<IntegrationsController> logger)
    {
        _integrationAppService = integrationAppService;
        _integrationManager = integrationManager;
        _logger = logger;
    }

    [HttpGet]
    [SwaggerOperation(Summary = "Get all integrations")]
    [SwaggerResponse(200, "Success", typeof(List<IntegrationDto>))]
    public async Task<ActionResult<List<IntegrationDto>>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var integrations = await _integrationAppService.GetAllAsync(cancellationToken);
            return Ok(integrations);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while getting all integrations");
            return StatusCode(500, "An error occurred while retrieving integrations.");
        }
    }

    [HttpGet("{id:guid}")]
    [SwaggerOperation(Summary = "Get integration by ID")]
    [SwaggerResponse(200, "Success", typeof(IntegrationDto))]
    [SwaggerResponse(404, "Integration not found")]
    public async Task<ActionResult<IntegrationDto>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        try
        {
            var integration = await _integrationAppService.GetByIdAsync(id, cancellationToken);
            if (integration == null)
            {
                _logger.LogWarning("Integration with ID {IntegrationId} not found", id);
                return NotFound($"Integration with ID {id} not found.");
            }
            return Ok(integration);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while getting integration by ID {IntegrationId}", id);
            return StatusCode(500, "An error occurred while retrieving the integration.");
        }
    }

    [HttpGet("by-type/{integrationType}")]
    [SwaggerOperation(Summary = "Get integrations by type")]
    [SwaggerResponse(200, "Success", typeof(List<IntegrationDto>))]
    public async Task<ActionResult<List<IntegrationDto>>> GetByIntegrationTypeAsync(
        [FromRoute] IntegrationType integrationType, 
        CancellationToken cancellationToken = default)
    {
        try
        {
            var integrations = await _integrationAppService.GetByIntegrationTypeAsync(integrationType, cancellationToken);
            return Ok(integrations);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while getting integrations by type {IntegrationType}", integrationType);
            return StatusCode(500, "An error occurred while retrieving integrations by type.");
        }
    }

    [HttpPost]
    [SwaggerOperation(Summary = "Create a new integration")]
    [SwaggerResponse(201, "Integration created successfully", typeof(IntegrationDto))]
    [SwaggerResponse(400, "Invalid request")]
    [SwaggerResponse(409, "Integration with same name already exists")]
    public async Task<ActionResult<IntegrationDto>> CreateAsync(
        [FromBody] CreateIntegrationDto dto, 
        CancellationToken cancellationToken = default)
    {
        try
        {
            var integration = await _integrationAppService.CreateAsync(dto, cancellationToken);
            return Created($"/api/Integrations/{integration.Id}", integration);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Invalid argument when creating integration: {Message}", ex.Message);
            return BadRequest(ex.Message);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Conflict when creating integration: {Message}", ex.Message);
            return Conflict(ex.Message);
        }
    }

    [HttpPut("{id:guid}")]
    [SwaggerOperation(Summary = "Update an integration")]
    [SwaggerResponse(200, "Integration updated successfully", typeof(IntegrationDto))]
    [SwaggerResponse(400, "Invalid request")]
    [SwaggerResponse(404, "Integration not found")]
    public async Task<ActionResult<IntegrationDto>> UpdateAsync(
        Guid id,
        [FromBody] UpdateIntegrationDto dto, 
        CancellationToken cancellationToken = default)
    {
        try
        {
            var integration = await _integrationAppService.UpdateAsync(id, dto, cancellationToken);
            if (integration == null)
            {
                return NotFound($"Integration with ID {id} not found.");
            }
            return Ok(integration);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Invalid argument when updating integration {IntegrationId}: {Message}", id, ex.Message);
            return BadRequest(ex.Message);
        }
    }

    [HttpDelete("{id:guid}")]
    [SwaggerOperation(Summary = "Delete an integration")]
    [SwaggerResponse(204, "Integration deleted successfully")]
    [SwaggerResponse(404, "Integration not found")]
    public async Task<ActionResult> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        try
        {
            var success = await _integrationAppService.DeleteAsync(id, cancellationToken);
            if (!success)
            {
                _logger.LogWarning("Attempted to delete non-existent integration with ID {IntegrationId}", id);
                return NotFound($"Integration with ID {id} not found.");
            }
            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while deleting integration {IntegrationId}", id);
            return StatusCode(500, "An error occurred while deleting the integration.");
        }
    }

    [HttpHead("{id:guid}")]
    [SwaggerOperation(Summary = "Check if integration exists")]
    [SwaggerResponse(200, "Integration exists")]
    [SwaggerResponse(404, "Integration not found")]
    public async Task<ActionResult> ExistsAsync(Guid id, CancellationToken cancellationToken = default)
    {
        try
        {
            var integration = await _integrationAppService.GetByIdAsync(id, cancellationToken);
            return integration != null ? Ok() : NotFound();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while checking if integration {IntegrationId} exists", id);
            return StatusCode(500, "An error occurred while checking integration existence.");
        }
    }

    /// <summary>
    /// Get integration by name
    /// </summary>
    [HttpGet("by-name/{name}")]
    [SwaggerOperation(Summary = "Get integration by name")]
    [SwaggerResponse(200, "Success", typeof(IntegrationDto))]
    [SwaggerResponse(404, "Integration not found")]
    public async Task<ActionResult<IntegrationDto>> GetByNameAsync(string name, CancellationToken cancellationToken = default)
    {
        try
        {
            var integration = await _integrationManager.GetByNameAsync(name, cancellationToken);
            if (integration == null)
            {
                _logger.LogWarning("Integration with name {IntegrationName} not found", name);
                return NotFound($"Integration with name {name} not found.");
            }
            var dto = new IntegrationDto(
                Id: integration.Id,
                Name: integration.Name,
                IntegrationType: integration.Type,
                AuthMethod: integration.AuthMethod,
                ClientId: integration.ClientId,
                TokenEndpoint: integration.TokenEndpoint,
                CreatedAt: integration.CreatedAt,
                UpdatedAt: integration.UpdatedAt);
            return Ok(dto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while getting integration by name {IntegrationName}", name);
            return StatusCode(500, "An error occurred while retrieving the integration.");
        }
    }
} 