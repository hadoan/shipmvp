using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using ShipMvp.Core.Attributes;
using ShipMvp.Core.Modules;
using ShipMvp.Integration.SemanticKernel.Infrastructure;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace ShipMvp.Integration.SemanticKernel
{
    [Module]
    public class SemanticKernelModule : IModule
    {
        public void ConfigureServices(IServiceCollection services)
        {
            // Register core Semantic Kernel service
            services.AddScoped<ISemanticKernelService, SemanticKernelService>();
            
            // Register ActivitySource for LLM logging (as singleton since it's thread-safe)
            services.AddSingleton(sp => new ActivitySource("ShipMvp.LlmLogging"));
            
            // Configure OpenTelemetry for LLM logging
            ConfigureOpenTelemetry(services);
        }

        public void Configure(IApplicationBuilder app, IHostEnvironment env)
        {
            // Log OpenTelemetry configuration status
            var logger = app.ApplicationServices.GetService<ILogger<SemanticKernelModule>>();
            if (env.IsDevelopment())
            {
                logger?.LogInformation("SemanticKernel: OpenTelemetry configured for development environment with console exporter");
            }
            else
            {
                logger?.LogInformation("SemanticKernel: OpenTelemetry configured for production environment with Langfuse integration");
            }
        }
        
        private static void ConfigureOpenTelemetry(IServiceCollection services)
        {
            services.AddOpenTelemetry()
                .ConfigureResource(resource => resource
                    .AddService("ShipMvp.SemanticKernel", "1.0.0")
                    .AddAttributes(new Dictionary<string, object>
                    {
                        ["service.component"] = "semantic-kernel",
                        ["service.instance.id"] = Environment.MachineName
                    }))
                .WithTracing(tracing =>
                {
                    tracing
                        .AddSource("Microsoft.SemanticKernel")
                        .AddSource("ShipMvp.LlmLogging"); // Our custom LLM logging source

                    // Add our custom LLM telemetry processor
                    try
                    {
                        tracing.AddProcessor(sp => new LlmTelemetryProcessor(sp));
                    }
                    catch (Exception)
                    {
                        // If processor creation fails, continue without it
                        // The basic OpenTelemetry tracing will still work
                    }

                    // Configure environment-specific settings
                    var serviceProvider = services.BuildServiceProvider();
                    var environment = serviceProvider.GetService<IHostEnvironment>();
                    var configuration = serviceProvider.GetService<IConfiguration>();

                    if (environment?.IsDevelopment() == true)
                    {
                        // Development configuration
                        tracing
                            .SetSampler(new AlwaysOnSampler()) // Always sample in development
                            .AddConsoleExporter(); // Export to console for development debugging
                        
                        // Add debug logging
                        services.Configure<LoggerFilterOptions>(options =>
                        {
                            options.AddFilter("OpenTelemetry", LogLevel.Debug);
                            options.AddFilter("ShipMvp.Integration.SemanticKernel", LogLevel.Debug);
                        });
                    }
                    else
                    {
                        // Production configuration with Langfuse
                        var lfPublic = configuration?["Integrations:Langfuse:PublicKey"] ?? Environment.GetEnvironmentVariable("LANGFUSE_PUBLIC");
                        var lfSecret = configuration?["Integrations:Langfuse:SecretKey"] ?? Environment.GetEnvironmentVariable("LANGFUSE_SECRET");
                        var lfOtel = configuration?["Integrations:Langfuse:Endpoint"] ?? "https://cloud.langfuse.com/api/public/otel";

                        if (!string.IsNullOrWhiteSpace(lfPublic) && !string.IsNullOrWhiteSpace(lfSecret))
                        {
                            tracing
                                .SetSampler(new TraceIdRatioBasedSampler(0.1)) // Sample 10% in production
                                .AddOtlpExporter(o =>
                                {
                                    o.Endpoint = new Uri(lfOtel);
                                    o.Protocol = OpenTelemetry.Exporter.OtlpExportProtocol.HttpProtobuf;
                                    var basic = BuildLangfuseAuth(lfPublic, lfSecret);
                                    o.Headers = $"Authorization=Basic {basic}";
                                });
                        }
                        else
                        {
                            // Fallback to TraceIdRatioBasedSampler if no Langfuse credentials
                            tracing.SetSampler(new TraceIdRatioBasedSampler(0.01)); // Sample 1% without external export
                        }
                    }
                });
        }
        
        private static string BuildLangfuseAuth(string pub, string sec)
        {
            var bytes = System.Text.Encoding.ASCII.GetBytes($"{pub}:{sec}");
            return Convert.ToBase64String(bytes);
        }
    }
}
