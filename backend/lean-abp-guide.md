# Lean ABP Implementation Guide for Solo Developers

## Philosophy: Minimal ABP, Maximum Value

Instead of full ABP Framework complexity, we'll implement ABP's core DDD principles with minimal overhead - perfect for solo developers who want clean architecture without enterprise bloat.

## Lean Architecture Principles

1. **4-Layer Structure** (not 7+ ABP layers)
2. **Convention over Configuration**
3. **Minimal Dependencies** (no full ABP packages)
4. **Single Module Structure** (expand later if needed)
5. **Built-in Patterns** (Repository, UoW, CQRS)

## Implementation Plan

### Phase 1: Lean Project Structure

```
src/
├── ShipMvp.Domain/           # Entities, Interfaces, Value Objects
├── ShipMvp.Application/      # Services, DTOs, Commands/Queries
├── ShipMvp.Infrastructure/   # Data Access, External Services
├── ShipMvp.Api/             # Controllers, Minimal APIs
├── ShipMvp.Shared/          # Constants, Enums, Common Types
└── ShipMvp.Host/            # Startup, DI Container
```

### Phase 2: Lean ABP Module System

Create a lightweight module system inspired by ABP but simpler:

```csharp
// Simple module interface
public interface IModule
{
    void ConfigureServices(IServiceCollection services);
    void Configure(IApplicationBuilder app);
}

// Auto-discovery with minimal attributes
[Module]
[DependsOn<InfrastructureModule>]
public class ApplicationModule : IModule
{
    public void ConfigureServices(IServiceCollection services)
    {
        services.AddApplicationServices();
    }
}
```

### Phase 3: Smart Conventions

Implement conventions that eliminate boilerplate:

```csharp
// Auto-register services by naming convention
services.AddServicesByConvention(); // Finds *Service, *Repository classes

// Auto-map DTOs with simple attributes
[AutoMap(typeof(Invoice))]
public class InvoiceDto { }

// Auto-generate API controllers
[AutoController]
public class InvoiceService : IInvoiceService { }
```

This approach gives you 80% of ABP's benefits with 20% of the complexity.
