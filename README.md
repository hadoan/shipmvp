
# ShipMvp

Ultra-lean SaaS starter kit with a modular .NET backend and modern React frontend.

## Features

- Modular Clean Architecture (Domain, Application, Infrastructure, API)
- .NET 8/9 backend with Entity Framework Core and SQLite (configurable for production)
- Stripe integration for subscription billing
- File upload system using Google Cloud Storage
- Modern React 18 + TypeScript frontend (Vite, TailwindCSS)
- ABP-inspired patterns and DDD-style modules

## Project Structure

```
backend/   # .NET backend (modular, DDD, Clean Architecture)
frontend/  # React TypeScript frontend (Vite, TailwindCSS)
```

## Getting Started

### Backend

```sh
cd backend
dotnet run --project src/ShipMvp.Api
```

- API docs: <http://localhost:5000/swagger>

### Frontend

```sh
cd frontend
npm install
npm run dev
```

- App: <http://localhost:5173>

## Example API Usage

Create an invoice:

```sh
curl -X POST http://localhost:5000/api/invoices -H "Content-Type: application/json" -d '{"customerName":"Acme","items":[{"description":"Widget","amount":100}]}'
```

## Architecture

- **Backend:** .NET 8/9, Clean Architecture, modular monolith, Stripe, GCP file storage
- **Frontend:** React 18, TypeScript, Vite, TailwindCSS
- **Docs:** See `ARCHITECTURE.md` for full details

## File Upload

- Drag-and-drop UI (see `frontend/src/pages/FileUploadDemo.tsx`)
- GCP Storage backend, signed URLs, public/private files

## Formatting & Conventions

- Prettier, ESLint, EditorConfig, C# Dev Kit (see `FORMATTING.md`)

---

For more details, see:

- `ARCHITECTURE.md` – full architecture and module breakdown
- `backend/README.md` – backend usage and DDD guide
- `frontend/README.md` – frontend setup

---
