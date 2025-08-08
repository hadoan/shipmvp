
# ShipMvp

Ultra-lean SaaS starter kit with a modular .NET backend and modern React frontend.

## Features

- Modular Clean Architecture (Domain, Application, Infrastructure, API)
- .NET 9 backend with Entity Framework Core and PostgreSQL
- **Full-stack orchestration with .NET Aspire 9.0**
- Containerized PostgreSQL with pgAdmin administration
- Service discovery, distributed tracing, and centralized monitoring
- Stripe integration for subscription billing
- File upload system using Google Cloud Storage
- Modern React 18 + TypeScript frontend (Vite, TailwindCSS)
- ABP-inspired patterns and DDD-style modules

## Project Structure

```text
backend/   # .NET backend (modular, DDD, Clean Architecture)
frontend/  # React TypeScript frontend (Vite, TailwindCSS)
```

## Getting Started

There are two ways to run the ShipMvp application:

### Option 1: Full-Stack with .NET Aspire (Recommended)

**Prerequisites:**

- .NET 9 SDK
- Docker Desktop (for PostgreSQL containers)

Run the complete application with orchestration, database, and monitoring:

```bash
cd backend/src/ShipMvp.AppHost
dotnet run
```

This will start:

- **PostgreSQL Database**: Containerized with data persistence
- **pgAdmin**: Database administration at your assigned port
- **API Backend**: ASP.NET Core Web API with Swagger
- **React Frontend**: Vite development server with hot reload
- **Aspire Dashboard**: Centralized monitoring and observability

**Access Points:**

- **Aspire Dashboard**: <https://localhost:17152> (monitoring, logs, traces)
- **API Swagger**: <http://localhost:5000/swagger> (API documentation)
- **Frontend App**: <http://localhost:8080> (React application)
- **pgAdmin**: Available through Aspire dashboard service discovery

### Option 2: Individual Services (Development)

For development work on individual components:

#### Backend Only

```bash
cd backend
dotnet run --project src/ShipMvp.Api
```

- API docs: <http://localhost:5000/swagger>

#### Frontend Only

```bash
cd frontend
npm install
npm run dev
```

- App: <http://localhost:5173>

## PostgreSQL Database Setup

### Option 1: Standalone PostgreSQL (Recommended for Development)

For development work or when you want to run PostgreSQL independently of Aspire:

```bash
# Start PostgreSQL container (matches Aspire configuration)
docker run --name shipmvp-postgres \
  -e POSTGRES_USER=postgres \
  -e POSTGRES_PASSWORD=ShipMVPPass123! \
  -e POSTGRES_DB=shipmvp \
  -e PGDATA=/var/lib/postgresql/data/pgdata \
  -p 5432:5432 \
  -v shipmvp-postgres-data:/var/lib/postgresql/data \
  --restart unless-stopped \
  -d postgres:latest
```

**Database Management:**

```bash
# Check container status
docker ps | grep postgres

# Stop PostgreSQL
docker stop shipmvp-postgres

# Start PostgreSQL (after stopping)
docker start shipmvp-postgres

# Remove PostgreSQL (WARNING: This deletes all data)
docker rm -f shipmvp-postgres
docker volume rm shipmvp-postgres-data
```

**Connect to PostgreSQL:**

```bash
# Using psql (if installed locally)
psql -h localhost -U postgres -d shipmvp

# Using Docker exec
docker exec -it shipmvp-postgres psql -U postgres -d shipmvp
```

**Run Migrations:**

With the standalone PostgreSQL running, you can create and apply migrations:

```bash
cd backend/src/ShipMvp.Application

# Create a new migration
dotnet ef migrations add YourMigrationName --startup-project ../ShipMvp.Api

# Apply migrations to database
dotnet ef database update --startup-project ../ShipMvp.Api
```

### Option 2: Aspire-Managed PostgreSQL

When using the full Aspire orchestration (`dotnet run` in `ShipMvp.AppHost`), PostgreSQL is automatically managed and configured. The Aspire setup can automatically detect and use the standalone PostgreSQL if it's already running.

**Connection Details:**

- **Host**: localhost
- **Port**: 5432
- **Database**: shipmvp
- **Username**: postgres
- **Password**: ShipMVPPass123!
- **Connection String**: `Host=localhost;Port=5432;Database=shipmvp;Username=postgres;Password=ShipMVPPass123!`

## Key Features

### .NET Aspire Orchestration

- **Service Discovery**: Automatic service registration and discovery
- **Distributed Tracing**: Full request flow visibility with OpenTelemetry
- **Centralized Logging**: Aggregated logs from all services in one dashboard
- **Health Monitoring**: Real-time health checks and status monitoring
- **Container Management**: Automated PostgreSQL and pgAdmin container lifecycle
- **Environment Configuration**: Centralized configuration and secrets management

### Database Integration

- **PostgreSQL**: Production-ready database with full ACID compliance
- **Entity Framework Core**: Code-first migrations with rich domain modeling
- **Connection Resilience**: Automatic retry policies and connection pooling
- **pgAdmin**: Web-based administration interface for database management

## Example API Usage

With Aspire running, create an invoice:

```bash
curl -X POST http://localhost:5000/api/invoices \
  -H "Content-Type: application/json" \
  -d '{"customerName":"Acme","items":[{"description":"Widget","amount":100}]}'
```

Access all endpoints via Swagger UI at <http://localhost:5000/swagger>

## Architecture

- **Backend:** .NET 9, Clean Architecture, modular monolith, PostgreSQL, Stripe, GCP file storage
- **Frontend:** React 18, TypeScript, Vite, TailwindCSS
- **Orchestration:** .NET Aspire 9.0 with service discovery and distributed tracing
- **Database:** PostgreSQL with pgAdmin, containerized via Docker
- **Monitoring:** Centralized logging, metrics, and health checks
- **Docs:** See `ARCHITECTURE.md` for full details

## File Upload

- Drag-and-drop UI (see `frontend/src/pages/FileUploadDemo.tsx`)
- GCP Storage backend, signed URLs, public/private files

## Formatting & Conventions

- Prettier, ESLint, EditorConfig, C# Dev Kit (see `FORMATTING.md`)

## Troubleshooting

### Aspire Issues

**Port conflicts:**

```bash
# Kill any processes using Aspire ports
lsof -ti:17152 | xargs kill -9
lsof -ti:20888 | xargs kill -9
```

**Container issues:**

```bash
# Clean up Docker containers
docker stop $(docker ps -aq) 2>/dev/null || true
docker system prune -f
```

**Database connection issues:**

- Check Aspire Dashboard for service status
- Verify PostgreSQL container is running and ready
- Check connection string injection in API logs

**PostgreSQL Issues:**

```bash
# Check if PostgreSQL is running
docker ps | grep postgres

# View PostgreSQL logs
docker logs shipmvp-postgres

# Restart PostgreSQL container
docker restart shipmvp-postgres

# Connect to PostgreSQL for debugging
docker exec -it shipmvp-postgres psql -U postgres -d shipmvp

# Reset PostgreSQL (WARNING: Deletes all data)
docker stop shipmvp-postgres
docker rm shipmvp-postgres
docker volume rm shipmvp-postgres-data
# Then run the docker run command again
```

**Migration Issues:**

```bash
# If migrations fail, check database connection
dotnet ef migrations list --startup-project ../ShipMvp.Api

# Remove last migration (if needed)
dotnet ef migrations remove --startup-project ../ShipMvp.Api

# Reset database to specific migration
dotnet ef database update MigrationName --startup-project ../ShipMvp.Api
```

### Development Tips

- Use Aspire Dashboard to monitor service health and logs
- PostgreSQL data persists across container restarts
- Frontend hot reload works seamlessly with Aspire orchestration
- API Swagger documentation includes all endpoints and models

---

For more details, see:

- `ARCHITECTURE.md` – full architecture and module breakdown
- `backend/README.md` – backend usage and DDD guide
- `frontend/README.md` – frontend setup
- `backend/src/ShipMvp.AppHost/README.md` – Aspire orchestration details

---
