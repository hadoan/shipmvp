namespace ShipMvp.Domain.Integrations;

public enum IntegrationType
{
    // Email Platforms
    Gmail = 1,
    Outlook = 2,
    
    // Calendar Platforms  
    GoogleCalendar = 3,
    OutlookCalendar = 4,
    
    // Social Media Platforms
    LinkedIn = 5,
    Facebook = 6,
    Twitter = 7,
    
    // Communication Platforms
    Slack = 8,
    Teams = 9,
    Zoom = 10,
    
    // CRM Platforms
    Salesforce = 11,
    HubSpot = 12,
    
    // Marketing Platforms
    Mailchimp = 13,
    SendGrid = 14,
    
    // Payment Platforms
    Stripe = 15,
    PayPal = 16,
    
    // Storage Platforms
    GoogleDrive = 17,
    OneDrive = 18,
    Dropbox = 19,
    
    // Analytics Platforms
    GoogleAnalytics = 20,
    
    // Project Management
    Asana = 21,
    Trello = 22,
    
    // AI/ML Platforms
    SemanticKernel = 23,
    
    // Other
    Other = 99
}

public enum AuthMethodType
{
    OAuth2 = 1,
    ApiKey = 2,
    BasicAuth = 3,
    BearerToken = 4,
    Custom = 5
} 