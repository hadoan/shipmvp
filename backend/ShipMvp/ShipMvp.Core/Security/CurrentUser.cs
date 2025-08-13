using Microsoft.AspNetCore.Http;
using System.Security.Claims;
using Microsoft.Extensions.Logging;

namespace ShipMvp.Core.Security;

/// <summary>
/// Default implementation of <see cref="ICurrentUser"/> that reads from <see cref="IHttpContextAccessor"/>.
/// </summary>
public sealed class CurrentUser : ICurrentUser
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly ILogger<CurrentUser> _logger;

    public CurrentUser(IHttpContextAccessor httpContextAccessor, ILogger<CurrentUser> logger)
    {
        _httpContextAccessor = httpContextAccessor;
        _logger = logger;
    }

    private ClaimsPrincipal? Principal => _httpContextAccessor.HttpContext?.User;

    public bool IsAuthenticated 
    { 
        get 
        {
            var isAuth = Principal?.Identity?.IsAuthenticated == true;
            _logger.LogDebug("CurrentUser.IsAuthenticated: {IsAuthenticated}, Principal: {Principal}, Identity: {Identity}, IdentityType: {IdentityType}, AuthenticationType: {AuthenticationType}", 
                isAuth, Principal != null, Principal?.Identity != null, Principal?.Identity?.GetType().Name, Principal?.Identity?.AuthenticationType);
            return isAuth;
        }
    }

    public Guid? Id
    {
        get
        {
            var idValue = this[ClaimTypes.NameIdentifier];
            var result = Guid.TryParse(idValue, out var id) ? id : (Guid?)null;
            _logger.LogDebug("CurrentUser.Id: {Id}, IdValue: {IdValue}, Principal: {Principal}", 
                result, idValue, Principal != null);
            return result;
        }
    }

    public string? UserName => this[ClaimTypes.Name];
    public string? Email => this[ClaimTypes.Email];

    public IReadOnlyList<string> Roles => Principal?.FindAll(ClaimTypes.Role).Select(c => c.Value).ToList() ?? new List<string>();

    public IEnumerable<Claim> Claims => Principal?.Claims ?? Array.Empty<Claim>();

    public string? this[string claimType] => Principal?.FindFirst(claimType)?.Value;
} 