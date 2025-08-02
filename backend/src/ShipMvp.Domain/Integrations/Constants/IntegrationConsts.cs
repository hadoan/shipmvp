namespace ShipMvp.Domain.Integrations.Constants;

public static class IntegrationConsts
{
    public static class Integration
    {
        public const int NameMaxLength = 100;
        public const int UserInfoMaxLength = 500;
        public const int ClientIdMaxLength = 200;
        public const int ClientSecretMaxLength = 500;
        public const int TokenEndpointMaxLength = 500;
    }

    public static class IntegrationCredential
    {
        public const int UserInfoMaxLength = 500;
    }

    public static class CredentialField
    {
        public const int KeyMaxLength = 100;
        public const int ValueMaxLength = 2000;
    }
} 