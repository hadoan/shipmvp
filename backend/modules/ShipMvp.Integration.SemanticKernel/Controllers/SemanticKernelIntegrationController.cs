using Microsoft.AspNetCore.Mvc;
using ShipMvp.Application.Integrations;
using ShipMvp.Domain.Integrations;
using ShipMvp.Domain.Integrations.Schemas;
using Microsoft.Extensions.Logging;
using ShipMvp.Core.Security;
using System.Threading.Tasks;
using System;
using System.Collections.Generic;
using ShipMvp.Core.Integrations;

namespace ShipMvp.Integration.SemanticKernel.Controllers;

/// <summary>
/// Controller for managing SemanticKernel integration status and operations
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class SemanticKernelIntegrationController : ControllerBase
{
    private readonly IIntegrationManager _integrationManager;
    private readonly IIntegrationRepository _integrationRepository;
    private readonly ILogger<SemanticKernelIntegrationController> _logger;
    private readonly ICurrentUser _currentUser;

    public SemanticKernelIntegrationController(
        IIntegrationManager integrationManager,
        IIntegrationRepository integrationRepository,
        ILogger<SemanticKernelIntegrationController> logger,
        ICurrentUser currentUser)
    {
        _integrationManager = integrationManager;
        _integrationRepository = integrationRepository;
        _logger = logger;
        _currentUser = currentUser;
    }

    [HttpGet("status")]
    public async Task<IActionResult> GetSemanticKernelIntegrationStatus()
    {
        try
        {
            var semanticKernelPlatform = await _integrationManager.GetByTypeAsync(IntegrationType.SemanticKernel.ToString());
            if (semanticKernelPlatform == null)
            {
                return Ok(new
                {
                    IsConnected = false,
                    Deployment = (string?)null,
                    ModelName = (string?)null,
                    PlatformType = IntegrationType.SemanticKernel,
                    Message = "SemanticKernel platform not configured"
                });
            }

            if (!_currentUser.IsAuthenticated || !_currentUser.Id.HasValue)
            {
                return Ok(new
                {
                    IsConnected = false,
                    Deployment = (string?)null,
                    ModelName = (string?)null,
                    PlatformType = IntegrationType.SemanticKernel,
                    Message = "User not authenticated"
                });
            }

            var credential = await _integrationRepository.GetByUserAndPlatformAsync(_currentUser.Id.Value, "SemanticKernel");
            
            if (credential != null)
            {
                return Ok(new
                {
                    IsConnected = true,
                    Deployment = credential.GetCredentialField(IntegrationCredentialSchemas.SemanticKernel.Deployment),
                    ModelName = credential.GetCredentialField(IntegrationCredentialSchemas.SemanticKernel.ModelName),
                    PlatformType = IntegrationType.SemanticKernel,
                    PlatformId = semanticKernelPlatform.Id,
                    CredentialId = credential.Id
                });
            }
            else
            {
                return Ok(new
                {
                    IsConnected = false,
                    Deployment = (string?)null,
                    ModelName = (string?)null,
                    PlatformType = IntegrationType.SemanticKernel,
                    PlatformId = semanticKernelPlatform.Id,
                    Message = "No SemanticKernel credentials found for user"
                });
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to check SemanticKernel integration status");
            return StatusCode(500, new { Error = "Failed to check SemanticKernel integration status" });
        }
    }

    [HttpPost("connect")]
    public async Task<IActionResult> ConnectSemanticKernel([FromBody] ConnectSemanticKernelDto request)
    {
        try
        {
            if (!_currentUser.IsAuthenticated || !_currentUser.Id.HasValue)
            {
                return Unauthorized(new { Error = "User not authenticated" });
            }

            var semanticKernelPlatform = await _integrationManager.GetByTypeAsync(IntegrationType.SemanticKernel.ToString());
            if (semanticKernelPlatform == null)
            {
                return BadRequest(new { Error = "SemanticKernel platform not configured" });
            }

            var credentials = new Dictionary<string, string>
            {
                { IntegrationCredentialSchemas.SemanticKernel.Deployment, request.Deployment },
                { IntegrationCredentialSchemas.SemanticKernel.ApiKey, request.ApiKey }
            };

            if (!string.IsNullOrEmpty(request.Endpoint))
            {
                credentials[IntegrationCredentialSchemas.SemanticKernel.Endpoint] = request.Endpoint;
            }

            if (!string.IsNullOrEmpty(request.ModelName))
            {
                credentials[IntegrationCredentialSchemas.SemanticKernel.ModelName] = request.ModelName;
            }

            if (!string.IsNullOrEmpty(request.Organization))
            {
                credentials[IntegrationCredentialSchemas.SemanticKernel.Organization] = request.Organization;
            }

            var credential = await _integrationManager.CreateOrUpdateGenericCredentialAsync(
                userId: _currentUser.Id.Value,
                integrationId: semanticKernelPlatform.Id,
                userInfo: request.Deployment, // Use deployment as userInfo
                credentials: credentials
            );

            return Ok(new
            {
                Success = true,
                CredentialId = credential.Id,
                Deployment = request.Deployment,
                ModelName = request.ModelName
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to connect SemanticKernel integration");
            return StatusCode(500, new { Error = "Failed to connect SemanticKernel integration" });
        }
    }

    [HttpDelete("disconnect")]
    public async Task<IActionResult> DisconnectSemanticKernel()
    {
        try
        {
            if (!_currentUser.IsAuthenticated || !_currentUser.Id.HasValue)
            {
                return Unauthorized(new { Error = "User not authenticated" });
            }

            var semanticKernelPlatform = await _integrationManager.GetByTypeAsync(IntegrationType.SemanticKernel.ToString());
            if (semanticKernelPlatform == null)
            {
                return BadRequest(new { Error = "SemanticKernel platform not configured" });
            }

            var credential = await _integrationRepository.GetByUserAndPlatformAsync(_currentUser.Id.Value, "SemanticKernel");
            if (credential == null)
            {
                return NotFound(new { Error = "No SemanticKernel credentials found for user" });
            }

            credential.MarkAsDeleted();
            await _integrationRepository.UpdateCredentialAsync(credential);

            return Ok(new { Success = true, Message = "SemanticKernel integration disconnected successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to disconnect SemanticKernel integration");
            return StatusCode(500, new { Error = "Failed to disconnect SemanticKernel integration" });
        }
    }
}

public record ConnectSemanticKernelDto(
    string Deployment,
    string ApiKey,
    string? Endpoint = null,
    string? ModelName = null,
    string? Organization = null
);
