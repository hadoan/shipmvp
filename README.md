
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
