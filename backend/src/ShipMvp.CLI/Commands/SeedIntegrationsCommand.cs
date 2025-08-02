using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using ShipMvp.Application.Integrations;
using ShipMvp.Core.Attributes;
using ShipMvp.Domain.Integrations;

namespace ShipMvp.CLI.Commands;

[UnitOfWork]
public class SeedIntegrationsCommand : ICommand
{
    private readonly ILogger<SeedIntegrationsCommand> _logger;
    private readonly IIntegrationAppService _integrationService;
    private readonly IConfiguration _configuration;

    public SeedIntegrationsCommand(
        ILogger<SeedIntegrationsCommand> logger,
        IIntegrationAppService integrationService,
        IConfiguration configuration)
    {
        _logger = logger;
        _integrationService = integrationService;
        _configuration = configuration;
    }

    public async Task ExecuteAsync(CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Starting database operations...");

        try
        {
            // 1. Show all tables
            await ShowAllTables(cancellationToken);

            // 2. Count records in each table
            await CountRecordsInTables(cancellationToken);

            // 3. Show existing integrations
            await ShowExistingIntegrations(cancellationToken);

            // 4. Seed integrations from configuration
            await SeedIntegrationsFromConfig(cancellationToken);

            // 5. Show updated integrations
            await ShowExistingIntegrations(cancellationToken);

            // 6. Run some sample queries
            await RunSampleQueries(cancellationToken);

            _logger.LogInformation("Database operations completed successfully.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during database operations");
            throw;
        }
    }

    private async Task ShowAllTables(CancellationToken cancellationToken)
    {
        _logger.LogInformation("=== SHOWING ALL TABLES ===");
        
        // This would require direct database access, but for now we'll show what we know
        var knownTables = new[]
        {
            "Users", "Integrations", "IntegrationCredentials", "SubscriptionPlans", 
            "UserSubscriptions", "SubscriptionUsages", "Files", "EmailMessages", 
            "EmailAttachments", "Invoices", "InvoiceItems"
        };

        foreach (var table in knownTables)
        {
            _logger.LogInformation("Table: {TableName}", table);
        }
    }

    private async Task CountRecordsInTables(CancellationToken cancellationToken)
    {
        _logger.LogInformation("=== COUNTING RECORDS IN TABLES ===");
        
        try
        {
            var integrations = await _integrationService.GetAllAsync(cancellationToken);
            _logger.LogInformation("Integrations count: {Count}", integrations.Count());
        }
        catch (Exception ex)
        {
            _logger.LogWarning("Could not count integrations: {Error}", ex.Message);
        }
    }

    private async Task ShowExistingIntegrations(CancellationToken cancellationToken)
    {
        _logger.LogInformation("=== EXISTING INTEGRATIONS ===");
        
        try
        {
            var integrations = await _integrationService.GetAllAsync(cancellationToken);
            foreach (var integration in integrations)
            {
                _logger.LogInformation("Integration: {Name} ({Type}) - Created: {CreatedAt}", 
                    integration.Name, integration.IntegrationType, integration.CreatedAt);
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning("Could not retrieve integrations: {Error}", ex.Message);
        }
    }

    private async Task SeedIntegrationsFromConfig(CancellationToken cancellationToken)
    {
        _logger.LogInformation("=== SEEDING INTEGRATIONS FROM CONFIG ===");

        var integrationsSection = _configuration.GetSection("Integrations");
        if (!integrationsSection.Exists())
        {
            _logger.LogWarning("No 'Integrations' section found in configuration. Skipping integration seeding.");
            return;
        }

        var integrations = integrationsSection.GetChildren();
        foreach (var integration in integrations)
        {
            try
            {
                var integrationName = integration.Key;
                var clientId = integration["ClientId"];
                var clientSecret = integration["ClientSecret"];
                var redirectUri = integration["RedirectUri"];

                if (string.IsNullOrEmpty(clientId) || string.IsNullOrEmpty(clientSecret))
                {
                    _logger.LogWarning("Integration {IntegrationName} is missing ClientId or ClientSecret, skipping...", integrationName);
                    continue;
                }

                var (integrationType, authMethod) = MapPlatformNameToTypeAndAuth(integrationName);

                // Check if an integration of this type already exists
                var existingIntegrations = await _integrationService.GetByIntegrationTypeAsync(integrationType, cancellationToken);
                var existingIntegration = existingIntegrations.FirstOrDefault();

                if (existingIntegration != null)
                {
                    // Update existing integration
                    _logger.LogInformation("Integration of type {IntegrationType} already exists (ID: {IntegrationId}), updating...", 
                        integrationType, existingIntegration.Id);

                    var updateDto = new UpdateIntegrationDto(
                        Name: integrationName,
                        ClientId: clientId,
                        ClientSecret: clientSecret,
                        TokenEndpoint: GetTokenEndpoint(integrationName)
                    );

                    var updated = await _integrationService.UpdateAsync(existingIntegration.Id, updateDto, cancellationToken);
                    if (updated != null)
                    {
                        _logger.LogInformation("Updated integration: {IntegrationName} (ID: {IntegrationId})", updated.Name, updated.Id);
                    }
                    else
                    {
                        _logger.LogError("Failed to update integration: {IntegrationName}", integrationName);
                    }
                }
                else
                {
                    // Create new integration
                    _logger.LogInformation("No integration of type {IntegrationType} exists, creating new one...", integrationType);

                    var createDto = new CreateIntegrationDto(
                        Name: integrationName,
                        IntegrationType: integrationType,
                        AuthMethod: authMethod,
                        ClientId: clientId,
                        ClientSecret: clientSecret,
                        TokenEndpoint: GetTokenEndpoint(integrationName)
                    );

                    var created = await _integrationService.CreateAsync(createDto, cancellationToken);
                    _logger.LogInformation("Created integration: {IntegrationName} with ID: {IntegrationId}", created.Name, created.Id);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing integration {IntegrationName}", integration.Key);
            }
        }
    }

    private async Task RunSampleQueries(CancellationToken cancellationToken)
    {
        _logger.LogInformation("=== RUNNING SAMPLE QUERIES ===");
        
        try
        {
            // Get integrations by type
            var gmailIntegrations = await _integrationService.GetByIntegrationTypeAsync(IntegrationType.Gmail, cancellationToken);
            _logger.LogInformation("Gmail integrations count: {Count}", gmailIntegrations.Count());

            // Get integration by platform
            var gmailIntegration = await _integrationService.GetByPlatformTypeAsync("gmail", cancellationToken);
            if (gmailIntegration != null)
            {
                _logger.LogInformation("Found Gmail integration: {Name} (ID: {Id})", gmailIntegration.Name, gmailIntegration.Id);
                
                // Check if integration exists by name
                var exists = await _integrationService.ExistsWithNameAsync(gmailIntegration.Name, null, cancellationToken);
                _logger.LogInformation("Integration '{Name}' exists: {Exists}", gmailIntegration.Name, exists);
            }

            // Get all integrations for listing
            var allIntegrations = await _integrationService.GetAllAsync(cancellationToken);
            _logger.LogInformation("Total integrations: {Count}", allIntegrations.Count());
            
            foreach (var integration in allIntegrations)
            {
                _logger.LogInformation("  - {Name} ({Type}) - Auth: {AuthMethod}", 
                    integration.Name, integration.IntegrationType, integration.AuthMethod);
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning("Error running sample queries: {Error}", ex.Message);
        }
    }

    private static (IntegrationType IntegrationType, AuthMethodType AuthMethod) MapPlatformNameToTypeAndAuth(string platformName)
    {
        return platformName.ToLowerInvariant() switch
        {
            "google" => (IntegrationType.Gmail, AuthMethodType.OAuth2),
            "gmail" => (IntegrationType.Gmail, AuthMethodType.OAuth2),
            "outlook" => (IntegrationType.Outlook, AuthMethodType.OAuth2),
            "microsoft" => (IntegrationType.Outlook, AuthMethodType.OAuth2),
            "linkedin" => (IntegrationType.LinkedIn, AuthMethodType.OAuth2),
            "facebook" => (IntegrationType.Facebook, AuthMethodType.OAuth2),
            "twitter" => (IntegrationType.Twitter, AuthMethodType.OAuth2),
            "stripe" => (IntegrationType.Stripe, AuthMethodType.ApiKey),
            "paypal" => (IntegrationType.PayPal, AuthMethodType.OAuth2),
            "salesforce" => (IntegrationType.Salesforce, AuthMethodType.OAuth2),
            "hubspot" => (IntegrationType.HubSpot, AuthMethodType.OAuth2),
            "mailchimp" => (IntegrationType.Mailchimp, AuthMethodType.OAuth2),
            "sendgrid" => (IntegrationType.SendGrid, AuthMethodType.ApiKey),
            "slack" => (IntegrationType.Slack, AuthMethodType.OAuth2),
            "teams" => (IntegrationType.Teams, AuthMethodType.OAuth2),
            "zoom" => (IntegrationType.Zoom, AuthMethodType.OAuth2),
            "dropbox" => (IntegrationType.Dropbox, AuthMethodType.OAuth2),
            "googledrive" => (IntegrationType.GoogleDrive, AuthMethodType.OAuth2),
            "onedrive" => (IntegrationType.OneDrive, AuthMethodType.OAuth2),
            "googleanalytics" => (IntegrationType.GoogleAnalytics, AuthMethodType.OAuth2),
            "googlecalendar" => (IntegrationType.GoogleCalendar, AuthMethodType.OAuth2),
            "outlookcalendar" => (IntegrationType.OutlookCalendar, AuthMethodType.OAuth2),
            "trello" => (IntegrationType.Trello, AuthMethodType.OAuth2),
            "asana" => (IntegrationType.Asana, AuthMethodType.OAuth2),
            "semantickernel" => (IntegrationType.SemanticKernel, AuthMethodType.ApiKey),
            "openai" => (IntegrationType.SemanticKernel, AuthMethodType.ApiKey), // Map OpenAI to SemanticKernel
            "azureopenai" => (IntegrationType.SemanticKernel, AuthMethodType.ApiKey), // Map Azure OpenAI to SemanticKernel
            "jira" => (IntegrationType.Other, AuthMethodType.OAuth2), // Jira not in enum, use Other
            _ => (IntegrationType.Other, AuthMethodType.OAuth2) // Default fallback
        };
    }

    private static string GetTokenEndpoint(string platformName)
    {
        return platformName.ToLowerInvariant() switch
        {
            "gmail" => "https://oauth2.googleapis.com/token",
            "google" => "https://oauth2.googleapis.com/token",
            "outlook" => "https://login.microsoftonline.com/common/oauth2/v2.0/token",
            "microsoft" => "https://login.microsoftonline.com/common/oauth2/v2.0/token",
            "linkedin" => "https://www.linkedin.com/oauth/v2/accessToken",
            "facebook" => "https://graph.facebook.com/v18.0/oauth/access_token",
            "twitter" => "https://api.twitter.com/2/oauth2/token",
            "stripe" => "", // API key doesn't use token endpoint
            "paypal" => "https://api.paypal.com/v1/oauth2/token",
            "salesforce" => "https://login.salesforce.com/services/oauth2/token",
            "hubspot" => "https://api.hubapi.com/oauth/v1/token",
            "mailchimp" => "https://login.mailchimp.com/oauth2/token",
            "sendgrid" => "", // API key doesn't use token endpoint
            "slack" => "https://slack.com/api/oauth.access",
            "teams" => "https://login.microsoftonline.com/common/oauth2/v2.0/token",
            "zoom" => "https://zoom.us/oauth/token",
            "dropbox" => "https://api.dropboxapi.com/oauth2/token",
            "googledrive" => "https://oauth2.googleapis.com/token",
            "onedrive" => "https://login.microsoftonline.com/common/oauth2/v2.0/token",
            "googleanalytics" => "https://oauth2.googleapis.com/token",
            "googlecalendar" => "https://oauth2.googleapis.com/token",
            "outlookcalendar" => "https://login.microsoftonline.com/common/oauth2/v2.0/token",
            "trello" => "https://trello.com/1/OAuthGetAccessToken",
            "asana" => "https://app.asana.com/-/oauth_token",
            "semantickernel" => "", // API key doesn't use token endpoint
            "openai" => "", // API key doesn't use token endpoint
            "azureopenai" => "", // API key doesn't use token endpoint
            "jira" => "https://auth.atlassian.com/oauth/token",
            _ => $"https://api.{platformName.ToLowerInvariant()}.com/oauth/token" // Default fallback
        };
    }

    public Task ExecuteAsync(string[] args, CancellationToken cancellationToken = default)
    {
        return ExecuteAsync(cancellationToken);
    }
}
