using Aspire.Hosting;

var builder = DistributedApplication.CreateBuilder(args);

// Add PostgreSQL database with specific password configuration
var postgresPassword = builder.AddParameter("postgres-password", value: "AspirePassword123!", secret: true);

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

builder.Build().Run();
