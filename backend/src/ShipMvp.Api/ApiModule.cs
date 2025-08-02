using Microsoft.OpenApi.Models;
using System.Reflection;
using ShipMvp.Application;
using ShipMvp.Core.Attributes;
using ShipMvp.Core.Modules;
using ShipMvp.Invoices.Application.Invoices;
using ShipMvp.Invoices.Domain.Entities;

namespace ShipMvp.Api;

[Module]
[DependsOn<ApplicationModule>]
public class ApiModule : IModule
{
    public void ConfigureServices(IServiceCollection services)
    {
        services.AddControllers();
        services.AddEndpointsApiExplorer();
        
        // Integration services are now registered in IntegrationModule
        
        // ABP-inspired Swagger configuration
        services.AddSwaggerGen(options =>
        {
            // Basic API Information
            options.SwaggerDoc("v1", new OpenApiInfo 
            { 
                Title = "ShipMvp API",
                Version = "v1",
                Description = "A lean ABP-inspired API for managing invoices and business operations",
                Contact = new OpenApiContact
                {
                    Name = "ShipMvp Team",
                    Email = "contact@shipmvp.com",
                    Url = new Uri("https://shipmvp.com")
                },
                License = new OpenApiLicense
                {
                    Name = "MIT License",
                    Url = new Uri("https://opensource.org/licenses/MIT")
                }
            });

            // Include XML documentation comments
            ConfigureXmlComments(options);
            
            // Configure schema generation
            options.CustomSchemaIds(type => type.FullName);
            
            // Document all endpoints by default (ABP-style)
            options.DocInclusionPredicate((docName, description) => true);
            
            // Add common response types
            ConfigureCommonResponses(options);
            
            // Enable annotations for better documentation
            options.EnableAnnotations();
            
            // Add operation filters for consistent API documentation
            options.OperationFilter<SwaggerResponseOperationFilter>();
        });
    }

    public void Configure(IApplicationBuilder app, IHostEnvironment env)
    {
        // Enable static file serving for custom CSS
        app.UseStaticFiles();
        
        // Always enable Swagger (ABP-style - available in all environments)
        app.UseSwagger(options =>
        {
            options.RouteTemplate = "swagger/{documentName}/swagger.json";
        });
        
        app.UseSwaggerUI(options =>
        {
            options.SwaggerEndpoint("/swagger/v1/swagger.json", "ShipMvp API v1");
            options.RoutePrefix = "swagger"; // Swagger UI at /swagger
            
            // ABP-inspired UI customizations
            options.DocumentTitle = "ShipMvp API Documentation";
            options.DefaultModelsExpandDepth(-1); // Hide models section by default
            options.DefaultModelExpandDepth(2);
            options.DisplayOperationId();
            options.DisplayRequestDuration();
            
            // Remove the custom CSS injection for now to fix the broken CSS
            // options.InjectStylesheet("/swagger-ui/custom.css");
        });

        // Add home page redirect to Swagger
        app.Use(async (context, next) =>
        {
            if (context.Request.Path == "/")
            {
                context.Response.Redirect("/swagger");
                return;
            }
            await next();
        });

        // Only use HTTPS redirection in production to avoid issues with local development
        if (!env.IsDevelopment())
        {
            app.UseHttpsRedirection();
        }
        
        app.UseRouting();
        app.UseAuthorization();
        app.UseEndpoints(endpoints =>
        {
            endpoints.MapControllers();
            
            // Add health check endpoint
            endpoints.MapGet("/health", async context =>
            {
                await context.Response.WriteAsync(System.Text.Json.JsonSerializer.Serialize(new 
                { 
                    status = "Healthy",
                    timestamp = DateTime.UtcNow,
                    version = Assembly.GetExecutingAssembly().GetName().Version?.ToString(),
                    environment = env.EnvironmentName
                }));
            });
        });
    }

    private static void ConfigureXmlComments(Swashbuckle.AspNetCore.SwaggerGen.SwaggerGenOptions options)
    {
        // Include XML comments from multiple assemblies
        var assemblies = new[]
        {
            Assembly.GetExecutingAssembly(), // API assembly
            typeof(IInvoiceService).Assembly, // Application assembly
            typeof(Invoice).Assembly // Domain assembly
        };

        foreach (var assembly in assemblies)
        {
            var xmlFilename = $"{assembly.GetName().Name}.xml";
            var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFilename);
            if (File.Exists(xmlPath))
            {
                options.IncludeXmlComments(xmlPath, true);
            }
        }
    }

    private static void ConfigureCommonResponses(Swashbuckle.AspNetCore.SwaggerGen.SwaggerGenOptions options)
    {
        // Configure common response schemas
        options.MapType<Microsoft.AspNetCore.Mvc.ProblemDetails>(() => new OpenApiSchema
        {
            Type = "object",
            Properties = new Dictionary<string, OpenApiSchema>
            {
                ["type"] = new() { Type = "string" },
                ["title"] = new() { Type = "string" },
                ["status"] = new() { Type = "integer" },
                ["detail"] = new() { Type = "string" },
                ["instance"] = new() { Type = "string" }
            }
        });
    }
}

// Custom operation filter for consistent response documentation
public class SwaggerResponseOperationFilter : Swashbuckle.AspNetCore.SwaggerGen.IOperationFilter
{
    public void Apply(OpenApiOperation operation, Swashbuckle.AspNetCore.SwaggerGen.OperationFilterContext context)
    {
        // Ensure 400 Bad Request is documented for all POST/PUT operations
        if (context.MethodInfo.GetCustomAttribute<Microsoft.AspNetCore.Mvc.HttpPostAttribute>() != null ||
            context.MethodInfo.GetCustomAttribute<Microsoft.AspNetCore.Mvc.HttpPutAttribute>() != null)
        {
            operation.Responses.TryAdd("400", new OpenApiResponse
            {
                Description = "Bad Request - Invalid input data"
            });
        }

        // Add 404 Not Found for GET operations with ID parameter
        if (context.MethodInfo.GetCustomAttribute<Microsoft.AspNetCore.Mvc.HttpGetAttribute>() != null &&
            context.MethodInfo.GetParameters().Any(p => p.Name?.ToLower().Contains("id") == true))
        {
            operation.Responses.TryAdd("404", new OpenApiResponse
            {
                Description = "Not Found - Resource does not exist"
            });
        }
    }
}
