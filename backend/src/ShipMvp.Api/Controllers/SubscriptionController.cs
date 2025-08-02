using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using ShipMvp.Application.Subscriptions;
using System.Security.Claims;
using ShipMvp.Core;
using ShipMvp.Domain.Shared.Constants;

namespace ShipMvp.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class SubscriptionController : ControllerBase
{
    private readonly ISubscriptionService _subscriptionService;
    private readonly ILogger<SubscriptionController> _logger;

    public SubscriptionController(
        ISubscriptionService subscriptionService,
        ILogger<SubscriptionController> logger)
    {
        _subscriptionService = subscriptionService;
        _logger = logger;
    }

    /// <summary>
    /// Get available subscription plans
    /// </summary>
    [HttpGet("plans")]
    [AllowAnonymous]
    public async Task<ActionResult<ApiResponse<List<SubscriptionPlanDto>>>> GetPlans()
    {
        try
        {
            var plans = await _subscriptionService.GetAvailablePlansAsync();
            return Ok(ApiResponse<List<SubscriptionPlanDto>>.Success(plans));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get subscription plans");
            return StatusCode(500, ApiResponse<List<SubscriptionPlanDto>>.MarkError("Failed to retrieve subscription plans"));
        }
    }

    /// <summary>
    /// Get current user's subscription details
    /// </summary>
    [HttpGet("current")]
    [Authorize(Policy = Policies.RequireReadOnly)]
    public async Task<ActionResult<ApiResponse<UserSubscriptionDto>>> GetCurrentSubscription()
    {
        try
        {
            var userId = GetCurrentUserId();
            var subscription = await _subscriptionService.GetUserSubscriptionAsync(userId);

            if (subscription == null)
            {
                return NotFound(ApiResponse<UserSubscriptionDto>.MarkError("Subscription not found"));
            }

            return Ok(ApiResponse<UserSubscriptionDto>.Success(subscription));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get user subscription for user {UserId}", GetCurrentUserId());
            return StatusCode(500, ApiResponse<UserSubscriptionDto>.MarkError("Failed to retrieve subscription"));
        }
    }

    /// <summary>
    /// Get current user's subscription usage
    /// </summary>
    [HttpGet("usage")]
    [Authorize(Policy = Policies.RequireReadOnly)]
    public async Task<ActionResult<ApiResponse<SubscriptionUsageDto>>> GetUsage()
    {
        try
        {
            var userId = GetCurrentUserId();
            var usage = await _subscriptionService.GetUsageAsync(userId);

            if (usage == null)
            {
                return NotFound(ApiResponse<SubscriptionUsageDto>.MarkError("Usage not found"));
            }

            return Ok(ApiResponse<SubscriptionUsageDto>.Success(usage));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get usage for user {UserId}", GetCurrentUserId());
            return StatusCode(500, ApiResponse<SubscriptionUsageDto>.MarkError("Failed to retrieve usage"));
        }
    }

    /// <summary>
    /// Create Stripe checkout session for subscription upgrade
    /// </summary>
    [HttpPost("checkout")]
    [Authorize] // Default policy applies - just requires authenticated user
    public async Task<ActionResult<ApiResponse<CreateCheckoutSessionResponse>>> CreateCheckoutSession(
        [FromBody] CreateCheckoutSessionRequest request)
    {
        try
        {
            var userId = GetCurrentUserId();
            var response = await _subscriptionService.CreateCheckoutSessionAsync(userId, request.PlanId, request.SuccessUrl, request.CancelUrl);

            return Ok(ApiResponse<CreateCheckoutSessionResponse>.Success(response));
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Invalid checkout request for user {UserId}", GetCurrentUserId());
            return BadRequest(ApiResponse<CreateCheckoutSessionResponse>.MarkError(ex.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create checkout session for user {UserId}", GetCurrentUserId());
            return StatusCode(500, ApiResponse<CreateCheckoutSessionResponse>.MarkError("Failed to create checkout session"));
        }
    }

    /// <summary>
    /// Create Stripe portal session for subscription management
    /// </summary>
    [HttpPost("portal")]
    [Authorize] // Default policy applies - just requires authenticated user
    public async Task<ActionResult<ApiResponse<CreatePortalSessionResponse>>> CreatePortalSession(
        [FromBody] CreatePortalSessionRequest request)
    {
        try
        {
            var userId = GetCurrentUserId();
            var response = await _subscriptionService.CreatePortalSessionAsync(userId, request.ReturnUrl);

            return Ok(ApiResponse<CreatePortalSessionResponse>.Success(response));
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Invalid portal request for user {UserId}", GetCurrentUserId());
            return BadRequest(ApiResponse<CreatePortalSessionResponse>.MarkError(ex.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create portal session for user {UserId}", GetCurrentUserId());
            return StatusCode(500, ApiResponse<CreatePortalSessionResponse>.MarkError("Failed to create portal session"));
        }
    }

    /// <summary>
    /// Track usage for a feature (e.g., invoice creation)
    /// </summary>
    [HttpPost("usage/track")]
    public async Task<ActionResult<ApiResponse<object>>> TrackUsage([FromBody] TrackUsageRequest request)
    {
        try
        {
            var userId = GetCurrentUserId();
            await _subscriptionService.TrackUsageAsync(userId, request.Feature, request.Amount);

            return Ok(ApiResponse<object>.Success(new { }));
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Usage limit exceeded for user {UserId}", GetCurrentUserId());
            return BadRequest(ApiResponse<object>.MarkError(ex.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to track usage for user {UserId}", GetCurrentUserId());
            return StatusCode(500, ApiResponse<object>.MarkError("Failed to track usage"));
        }
    }

    /// <summary>
    /// Check if user can use a specific feature
    /// </summary>
    [HttpGet("can-use/{feature}")]
    public async Task<ActionResult<ApiResponse<CanUseFeatureResponse>>> CanUseFeature(string feature)
    {
        try
        {
            var userId = GetCurrentUserId();
            var canUse = await _subscriptionService.CanUseFeatureAsync(userId, feature);

            return Ok(ApiResponse<CanUseFeatureResponse>.Success(new CanUseFeatureResponse(canUse)));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to check feature access for user {UserId}", GetCurrentUserId());
            return StatusCode(500, ApiResponse<CanUseFeatureResponse>.MarkError("Failed to check feature access"));
        }
    }

    private string GetCurrentUserId()
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId))
        {
            throw new UnauthorizedAccessException("User ID not found in claims");
        }
        return userId;
    }
}

// Request/Response DTOs
public record CreateCheckoutSessionRequest(string PlanId, string SuccessUrl, string CancelUrl);
public record CreatePortalSessionRequest(string ReturnUrl);
public record TrackUsageRequest(string Feature, int Amount = 1);
public record CanUseFeatureResponse(bool CanUse);
