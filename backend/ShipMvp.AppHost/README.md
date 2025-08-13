# ShipMvp with .NET Aspire

This project includes .NET Aspire support for local development orchestration and observability of the complete ShipMvp SaaS platform.

## Services Managed

### Infrastructure
- **PostgreSQL** - Main database with persistent data volumes
- **pgAdmin** - Database administration interface

### Applications
- **ShipMvp API** - ASP.NET Core Web API backend
- **ShipMvp Frontend** - Vite React frontend with TypeScript

## What's Added

- **ShipMvp.AppHost** - Aspire orchestration project that manages all services
- **Service Discovery** - Automatic service-to-service communication
- **OpenTelemetry** - Distributed tracing and metrics
- **Health Checks** - Application health monitoring
- **Environment Configuration** - Automatic API URL injection to frontend
- **PostgreSQL & Redis** - Managed database and caching services

## Running with Aspire

1. **Start the Aspire AppHost** (recommended for development):
   ```sh
   cd backend/src/ShipMvp.AppHost
   dotnet run
   ```

   Or use VS Code: Select ".NET Launch Aspire AppHost" from the debug menu.

2. **Access Services**:
   - **Aspire Dashboard**: <https://localhost:15888> - Service monitoring and logs
   - **API**: <http://localhost:5000> - Backend API with Swagger at `/swagger`
   - **Frontend**: <http://localhost:8080> - React frontend application
   - **pgAdmin**: <http://localhost:5433> - PostgreSQL administration

3. **Development Workflow**: All services start automatically in dependency order with proper environment configuration

## What Aspire Provides

- **PostgreSQL**: Automatically started with pgAdmin
- **Service Discovery**: Services find each other automatically
- **Observability**: Logs, metrics, and distributed tracing
- **Health Checks**: Available at `/health` and `/alive` endpoints

## Running Without Aspire

You can still run the API directly for simpler debugging:

```sh
cd backend/src/ShipMvp.Api
dotnet run
```

Or use VS Code: Select ".NET Launch ShipMvp.Api" from the debug menu.

## Configuration

The AppHost configures:

- Database connection strings automatically
- Service-to-service communication
- Development environment settings
- OpenTelemetry exporters

## Next Steps

- Configure your connection strings to use the Aspire-provided PostgreSQL
- Add health checks for your custom services
- Use the dashboard to monitor application performance
- Add more services (like message queues) through the AppHost if needed

## Learn More

- [.NET Aspire Documentation](https://learn.microsoft.com/en-us/dotnet/aspire/)
- [Aspire Dashboard Guide](https://learn.microsoft.com/en-us/dotnet/aspire/fundamentals/dashboard/)
