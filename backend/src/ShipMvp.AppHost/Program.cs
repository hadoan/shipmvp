using Aspire.Hosting;
using Microsoft.Extensions.Configuration;

var builder = DistributedApplication.CreateBuilder(args);

// Check if external PostgreSQL is available, otherwise use Aspire-managed PostgreSQL
var useExternalPostgres = builder.Configuration.GetValue<bool>("UseExternalPostgres");

if (useExternalPostgres)
{
    // Use external PostgreSQL (running in Docker or elsewhere)
    var connectionString = builder.Configuration.GetConnectionString("PostgreSQL")
        ?? "Host=localhost;Port=5432;Database=shipmvp;Username=postgres;Password=ShipMVPPass123!";

    // For external connection, we need to configure the connection string directly in the API project
    // The API project will use the connection string from its own appsettings.json
    var api = builder.AddProject<Projects.ShipMvp_Api>("shipmvp-api")
        .WithEnvironment("ASPNETCORE_ENVIRONMENT", "Development")
        .WithEnvironment("ConnectionStrings__DefaultConnection", connectionString);

    // Add frontend Vite React app
    var frontend = builder.AddNpmApp("shipmvp-frontend", "../../../frontend")
        .WithReference(api)
        .WithHttpEndpoint(port: 8080, env: "PORT")
        .WithExternalHttpEndpoints()
        .WithEnvironment("VITE_API_BASE_URL", api.GetEndpoint("http"))
        .WithEnvironment("VITE_ENVIRONMENT", "development")
        .WithEnvironment("VITE_DEBUG", "true");
}
else
{
    // Use Aspire-managed PostgreSQL
    var postgresPassword = builder.AddParameter("postgres-password", value: "ShipMVPPass123!", secret: true);
    var postgres = builder.AddPostgres("postgres", password: postgresPassword)
        .WithDataVolume()
        .WithPgAdmin();
    var database = postgres.AddDatabase("shipmvp");

    // Add the API project with database reference
    var api = builder.AddProject<Projects.ShipMvp_Api>("shipmvp-api")
        .WithReference(database)
        .WithEnvironment("ASPNETCORE_ENVIRONMENT", "Development");

    // Add frontend Vite React app
    var frontend = builder.AddNpmApp("shipmvp-frontend", "../../../frontend")
        .WithReference(api)
        .WithHttpEndpoint(port: 8080, env: "PORT")
        .WithExternalHttpEndpoints()
        .WithEnvironment("VITE_API_BASE_URL", api.GetEndpoint("http"))
        .WithEnvironment("VITE_ENVIRONMENT", "development")
        .WithEnvironment("VITE_DEBUG", "true");
}

builder.Build().Run();
