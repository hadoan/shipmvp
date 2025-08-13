namespace ShipMvp.Domain.Integrations.Schemas;

public static class IntegrationCredentialSchemas
{
    public static class Gmail
    {
        public const string AccessToken = "access_token";
        public const string RefreshToken = "refresh_token";
        public const string TokenType = "token_type";
        public const string Scope = "scope";
        public const string ExpiresAt = "expires_at";
    }

    public static class SemanticKernel
    {
        public const string Deployment = "deployment";
        public const string ApiKey = "api_key";
        public const string Endpoint = "endpoint";
        public const string ModelName = "model_name";
        public const string Organization = "organization";
    }

    public static class OpenAI
    {
        public const string ApiKey = "api_key";
        public const string Organization = "organization";
        public const string ModelName = "model_name";
    }

    public static class AzureOpenAI
    {
        public const string ApiKey = "api_key";
        public const string Endpoint = "endpoint";
        public const string Deployment = "deployment";
        public const string ApiVersion = "api_version";
    }

    public static class Anthropic
    {
        public const string ApiKey = "api_key";
        public const string ModelName = "model_name";
    }

    public static class GenericOAuth
    {
        public const string AccessToken = "access_token";
        public const string RefreshToken = "refresh_token";
        public const string TokenType = "token_type";
        public const string Scope = "scope";
        public const string ExpiresAt = "expires_at";
    }

    public static class GenericApiKey
    {
        public const string ApiKey = "api_key";
        public const string ApiSecret = "api_secret";
        public const string Endpoint = "endpoint";
    }
} 