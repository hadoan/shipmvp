# ShipMvp.Api

ASP.NET Core Web API host for the ShipMvp SaaS platform. This project exposes all backend HTTP endpoints, handles authentication, and provides API documentation via Swagger.

## Main Features

- Modular ABP-inspired architecture (via `IModule`)
- JWT authentication and authorization
- Swagger/OpenAPI documentation
- CORS configuration for frontend integration
- Loads and wires up all application and domain modules
- Centralized logging and configuration

## Getting Started

1. **Configure Environment**
   - Set up `appsettings.json` for JWT, Stripe, CORS, and other settings.
   - Ensure dependent projects (Application, Domain, Invoices, etc.) are built.

2. **Run the API**

   ```sh
   dotnet run --project src/ShipMvp.Api
   ```

   The API will be available at [http://localhost:5000](http://localhost:5000)

3. **API Documentation**
   - Swagger UI: [http://localhost:5000/swagger](http://localhost:5000/swagger)

4. **Authentication**
   - Uses JWT Bearer tokens. Configure issuer, audience, and key in `appsettings.json`.

5. **CORS**
   - Allowed origins are set in `appsettings.json` under `App:CorsOrigins`.

## Project Structure

- `Program.cs` – Main entry, DI, authentication, module loading
- `ApiModule.cs` – Registers controllers, Swagger, and API services
- `HostModule.cs` – Host-level configuration (CORS, logging)
- `Controllers/` – (If present) Custom API controllers
- `appsettings.json` – Configuration for API, JWT, Stripe, etc.

## Extending the API

- Add new modules or controllers by referencing them in `AddModules` in `Program.cs`.
- Register new services in the appropriate module's `ConfigureServices`.

---

For more details, see the root and backend `README.md` files.
