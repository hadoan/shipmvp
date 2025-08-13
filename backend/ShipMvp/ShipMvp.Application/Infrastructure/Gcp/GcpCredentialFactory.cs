using System;
using System.IO;
using Google.Apis.Auth.OAuth2;
using Microsoft.Extensions.Configuration;

namespace ShipMvp.Application.Infrastructure.Gcp
{
    public static class GcpCredentialFactory
    {
        public static GoogleCredential Create(IConfiguration configuration)
        {
            var credentialsPath = configuration["Gcp:CredentialsPath"];
            if (!string.IsNullOrEmpty(credentialsPath) && File.Exists(credentialsPath))
            {
                var json = File.ReadAllText(credentialsPath);
                return GoogleCredential.FromJson(json);
            }
            // Fallback to default credentials
            return GoogleCredential.GetApplicationDefault();
        }
    }
}
