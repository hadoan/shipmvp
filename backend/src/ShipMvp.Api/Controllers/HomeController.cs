using Microsoft.AspNetCore.Mvc;

namespace ShipMvp.Api.Controllers;

[ApiController]
[Route("")]
public class HomeController : ControllerBase
{
    /// <summary>
    /// Welcome endpoint
    /// </summary>
    [HttpGet]
    public IActionResult Get()
    {
        return Ok(new
        {
            Message = "Welcome to ShipMvp API",
            Version = "1.0.0",
            Architecture = "ShipMvp",
            Endpoints = new
            {
                Health = "/health",
                Invoices = "/api/invoices",
                Swagger = "/swagger"
            }
        });
    }

    /// <summary>
    /// Health check endpoint
    /// </summary>
    [HttpGet("health")]
    public IActionResult Health()
    {
        return Ok(new
        {
            Status = "Healthy",
            Timestamp = DateTime.UtcNow,
            Environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production"
        });
    }
}
