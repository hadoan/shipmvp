using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace ShipMvp.Api.Host;

/// <summary>
/// Configures JWT Bearer options using the options pattern
/// </summary>
public class JwtBearerOptionsSetup : IConfigureNamedOptions<JwtBearerOptions>
{
    private readonly IConfiguration _configuration;

    public JwtBearerOptionsSetup(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public void Configure(string? name, JwtBearerOptions options)
    {
        if (name == JwtBearerDefaults.AuthenticationScheme)
        {
            Configure(options);
        }
    }

    public void Configure(JwtBearerOptions options)
    {
        var jwtKey = _configuration["Jwt:Key"] ?? "default-fallback-key-for-development-only-32-chars-minimum";
        var jwtIssuer = _configuration["Jwt:Issuer"] ?? "ShipMvp";
        var jwtAudience = _configuration["Jwt:Audience"] ?? "ShipMvp";

        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtIssuer,
            ValidAudience = jwtAudience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
            ClockSkew = TimeSpan.Zero,
            // Map role claims properly
            RoleClaimType = "role",
            NameClaimType = "name"
        };

        options.Events = new JwtBearerEvents
        {
            OnAuthenticationFailed = context =>
            {
                Console.WriteLine($"JWT Authentication failed: {context.Exception.Message}");
                return Task.CompletedTask;
            },
            OnTokenValidated = context =>
            {
                Console.WriteLine("JWT Token validated successfully");
                return Task.CompletedTask;
            },
            OnChallenge = context =>
            {
                Console.WriteLine($"JWT Authentication challenge: {context.Error}, {context.ErrorDescription}");
                return Task.CompletedTask;
            }
        };
    }
}
