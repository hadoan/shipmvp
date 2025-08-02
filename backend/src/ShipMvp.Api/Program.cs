using ShipMvp.Api;
using ShipMvp.Application;
using ShipMvp.Invoices;

using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using ShipMvp.Core.Modules;
using Npgsql;

NpgsqlConnection.GlobalTypeMapper.UseJsonNet();

var builder = WebApplication.CreateBuilder(args);

// Configure logging
builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.AddDebug();
builder.Logging.AddConfiguration(builder.Configuration.GetSection("Logging"));

// Lean ABP-style module loading
builder.Services.AddModules(
    typeof(HostModule).Assembly,
    typeof(ApplicationModule).Assembly,
    typeof(ApiModule).Assembly,
    typeof(InvoiceModule).Assembly,

    typeof(Program).Assembly);

// Configure authentication
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidateAudience = true,
            ValidAudience = builder.Configuration["Jwt:Audience"],
            ValidateLifetime = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"])),
            ValidateIssuerSigningKey = true,
            NameClaimType = System.Security.Claims.ClaimTypes.Name,
            RoleClaimType = System.Security.Claims.ClaimTypes.Role,
            RequireSignedTokens = true
        };

        // Add event handlers for debugging
        options.Events = new JwtBearerEvents
        {
            OnAuthenticationFailed = context =>
            {
                var logger = context.HttpContext.RequestServices.GetRequiredService<ILogger<Program>>();
                logger.LogError("JWT Authentication failed: {Exception}", context.Exception);
                logger.LogError("JWT Authentication failure reason: {Failure}", context.Exception.Message);
                
                // Log the JWT configuration for debugging
                var configuration = context.HttpContext.RequestServices.GetRequiredService<IConfiguration>();
                logger.LogError("JWT Config - Issuer: {Issuer}, Audience: {Audience}, KeyLength: {KeyLength}", 
                    configuration["Jwt:Issuer"], 
                    configuration["Jwt:Audience"], 
                    configuration["Jwt:Key"]?.Length ?? 0);
                
                return Task.CompletedTask;
            },
            OnTokenValidated = context =>
            {
                var logger = context.HttpContext.RequestServices.GetRequiredService<ILogger<Program>>();
                logger.LogInformation("JWT Token validated successfully for user: {UserName}", 
                    context.Principal?.Identity?.Name);
                logger.LogInformation("JWT Token claims count: {ClaimsCount}", context.Principal?.Claims.Count() ?? 0);
                logger.LogInformation("JWT Identity.IsAuthenticated: {IsAuthenticated}", context.Principal?.Identity?.IsAuthenticated);
                logger.LogInformation("JWT Identity.AuthenticationType: {AuthenticationType}", context.Principal?.Identity?.AuthenticationType);
                
                // Log all claims for debugging
                foreach (var claim in context.Principal?.Claims ?? Array.Empty<System.Security.Claims.Claim>())
                {
                    logger.LogInformation("JWT Claim: {Type} = {Value}", claim.Type, claim.Value);
                }
                
                return Task.CompletedTask;
            },
            OnChallenge = context =>
            {
                var logger = context.HttpContext.RequestServices.GetRequiredService<ILogger<Program>>();
                logger.LogWarning("JWT Challenge issued: {Error}", context.Error);
                return Task.CompletedTask;
            }
        };
    });

var app = builder.Build();

// IMPORTANT: Apply CORS first, before any other middleware
app.UseCors(builder =>
{
    var configuration = app.Services.GetRequiredService<IConfiguration>();
    var logger = app.Services.GetRequiredService<ILogger<Program>>();
    
    // Get CORS origins from configuration
    var corsOrigins = configuration["App:CorsOrigins"];
    var allowedOrigins = new List<string>();
    
    if (!string.IsNullOrWhiteSpace(corsOrigins))
    {
        allowedOrigins = corsOrigins
            .Split(',', StringSplitOptions.RemoveEmptyEntries)
            .Select(o => o.Trim())
            .Where(o => !string.IsNullOrWhiteSpace(o))
            .Distinct()
            .ToList();
    }
    
    // Add default development origins if none configured
    if (!allowedOrigins.Any())
    {
        allowedOrigins = new List<string>
        {
            "http://localhost:3000",
            "http://localhost:5173", 
            "http://localhost:8080",
            "http://localhost:8081"
        };
    }
    
    logger.LogInformation("CORS: Configuring allowed origins: {Origins}", string.Join(", ", allowedOrigins));
    
    builder
        .WithOrigins(allowedOrigins.ToArray())
        .AllowAnyMethod()
        .AllowAnyHeader()
        .AllowCredentials()
        .WithExposedHeaders("Content-Disposition", "Content-Length", "X-Total-Count")
        .SetPreflightMaxAge(TimeSpan.FromMinutes(30));
});

// Add request logging middleware for debugging
app.Use(async (context, next) =>
{
    var logger = context.RequestServices.GetRequiredService<ILogger<Program>>();
    logger.LogInformation("=== REQUEST STARTED ===");
    logger.LogInformation("Request Path: {Path}", context.Request.Path);
    logger.LogInformation("Request Method: {Method}", context.Request.Method);
    logger.LogInformation("Authorization Header: {AuthHeader}", 
        context.Request.Headers["Authorization"].FirstOrDefault()?.Substring(0, Math.Min(50, context.Request.Headers["Authorization"].FirstOrDefault()?.Length ?? 0)) + "...");
    
    await next();
    
    logger.LogInformation("=== REQUEST COMPLETED ===");
});

// IMPORTANT: Authentication middleware must be registered BEFORE modules
app.UseAuthentication();
app.UseAuthorization();

// Configure modules (this will apply other middleware)
app.ConfigureModules(app.Environment);

app.Run();

// Make Program class accessible for testing
public partial class Program { }
