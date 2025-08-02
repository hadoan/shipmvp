# ShipMvp Migration Plan & Implementation Checklist

## Migration Strategy

This checklist provides a step-by-step approach to align ShipMvp with ABP Framework standards while maintaining system stability throughout the migration.

## Phase 1: Project Structure Reorganization (Effort: Medium, 1 day)

### 1.1 Create ABP-Compliant Project Structure

**Current Structure:**

```
src/Modules/Invoice/
├── Invoice.Core/
└── Invoice.Infrastructure/
```

**Target ABP Structure:**

```
src/Modules/Invoice/
├── ShipMvp.Invoice.Domain.Shared/
├── ShipMvp.Invoice.Domain/
├── ShipMvp.Invoice.Application.Contracts/
├── ShipMvp.Invoice.Application/
├── ShipMvp.Invoice.EntityFrameworkCore/
├── ShipMvp.Invoice.HttpApi/
└── ShipMvp.Invoice.HttpApi.Client/
```

#### Checklist:

- [ ] Create `ShipMvp.Invoice.Domain.Shared` project
- [ ] Create `ShipMvp.Invoice.Domain` project
- [ ] Create `ShipMvp.Invoice.Application.Contracts` project
- [ ] Create `ShipMvp.Invoice.Application` project
- [ ] Create `ShipMvp.Invoice.EntityFrameworkCore` project
- [ ] Create `ShipMvp.Invoice.HttpApi` project
- [ ] Update solution file with new project references

### 1.2 Update Project References

#### Package References:

```xml
<!-- Domain.Shared -->
<PackageReference Include="Volo.Abp.Core" Version="8.0.0" />

<!-- Domain -->
<ProjectReference Include="../ShipMvp.Invoice.Domain.Shared/ShipMvp.Invoice.Domain.Shared.csproj" />
<PackageReference Include="Volo.Abp.Ddd.Domain" Version="8.0.0" />

<!-- Application.Contracts -->
<ProjectReference Include="../ShipMvp.Invoice.Domain.Shared/ShipMvp.Invoice.Domain.Shared.csproj" />
<PackageReference Include="Volo.Abp.Ddd.Application.Contracts" Version="8.0.0" />

<!-- Application -->
<ProjectReference Include="../ShipMvp.Invoice.Domain/ShipMvp.Invoice.Domain.csproj" />
<ProjectReference Include="../ShipMvp.Invoice.Application.Contracts/ShipMvp.Invoice.Application.Contracts.csproj" />
<PackageReference Include="Volo.Abp.Ddd.Application" Version="8.0.0" />
```

#### Checklist:

- [ ] Add ABP package references to each project
- [ ] Configure proper project-to-project references
- [ ] Ensure dependency flow: Shared ← Domain ← Application.Contracts ← Application
- [ ] Test build after each project addition

## Phase 2: Module Definition Migration (Effort: Small, 0.5 days)

### 2.1 Migrate to ABP Module System

**Current (Custom):**

```csharp
public class InvoiceModule : IAppModule
{
    public void ConfigureServices(IServiceCollection services) { }
    public void MapEndpoints(IEndpointRouteBuilder endpoints) { }
}
```

**Target (ABP):**

```csharp
[DependsOn(typeof(AbpDddDomainModule))]
[DependsOn(typeof(AbpEntityFrameworkCoreModule))]
public class ShipMvpInvoiceDomainModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        // Configure domain services
    }
}

[DependsOn(typeof(ShipMvpInvoiceDomainModule))]
[DependsOn(typeof(AbpDddApplicationModule))]
public class ShipMvpInvoiceApplicationModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        // Configure application services
    }
}
```

#### Checklist:

- [ ] Create `ShipMvpInvoiceDomainModule.cs` in Domain project
- [ ] Create `ShipMvpInvoiceApplicationModule.cs` in Application project
- [ ] Create `ShipMvpInvoiceEntityFrameworkCoreModule.cs` in EF Core project
- [ ] Create `ShipMvpInvoiceHttpApiModule.cs` in HttpApi project
- [ ] Add `[DependsOn]` attributes for module dependencies
- [ ] Configure AutoMapper profiles in Application module
- [ ] Remove legacy `IAppModule` interface and implementations

### 2.2 Update Host Configuration

#### Current:

```csharp
builder.Services.AddAppModules(
    typeof(Program).Assembly,
    typeof(InvoiceModule).Assembly);
```

#### Target:

```csharp
builder.Host.UseAbp();
// ABP will automatically discover and load modules via [DependsOn]
```

#### Checklist:

- [ ] Add ABP hosting package reference to Host project
- [ ] Create `ShipMvpHostModule` inheriting from `AbpModule`
- [ ] Add `[DependsOn(typeof(ShipMvpInvoiceApplicationModule))]` to host module
- [ ] Remove custom module loading logic
- [ ] Update `Program.cs` to use ABP initialization

## Phase 3: Layer Implementation (Effort: Medium, 1 day)

### 3.1 Domain Layer Migration

#### Move entities from Invoice.Core to Domain:

```csharp
// ShipMvp.Invoice.Domain/Entities/Invoice.cs
namespace ShipMvp.Invoice.Domain.Entities;

public class Invoice : AggregateRoot<Guid>
{
    public string CustomerName { get; private set; }
    public List<InvoiceItem> Items { get; private set; }

    // Proper DDD entity with behavior, not just data
}
```

#### Checklist:

- [ ] Move `Invoice` and `InvoiceItem` entities to Domain project
- [ ] Inherit from ABP's `AggregateRoot<T>` or `Entity<T>`
- [ ] Move `IInvoiceRepository` interface to Domain project
- [ ] Implement proper domain invariants and business rules
- [ ] Create domain events if needed
- [ ] Update namespace to `ShipMvp.Invoice.Domain`

### 3.2 Application Layer Implementation

#### Create Application Services:

```csharp
// ShipMvp.Invoice.Application.Contracts/IInvoiceAppService.cs
public interface IInvoiceAppService : IApplicationService
{
    Task<InvoiceDto> CreateAsync(CreateInvoiceDto input);
    Task<InvoiceDto> GetAsync(Guid id);
    Task<PagedResultDto<InvoiceDto>> GetListAsync(GetInvoiceListDto input);
}

// ShipMvp.Invoice.Application/InvoiceAppService.cs
public class InvoiceAppService : ApplicationService, IInvoiceAppService
{
    private readonly IInvoiceRepository _repository;

    public async Task<InvoiceDto> CreateAsync(CreateInvoiceDto input)
    {
        // Implementation using Domain entities
    }
}
```

#### Checklist:

- [ ] Create DTOs in Application.Contracts project
- [ ] Create application service interfaces in Application.Contracts
- [ ] Implement application services in Application project
- [ ] Configure AutoMapper for entity-to-DTO mapping
- [ ] Migrate MediatR handlers to application services
- [ ] Add input validation using ABP validation attributes
- [ ] Implement proper exception handling

### 3.3 Infrastructure Layer Migration

#### Entity Framework Core Implementation:

```csharp
// ShipMvp.Invoice.EntityFrameworkCore/InvoiceDbContext.cs
[ConnectionStringName("Invoice")]
public class InvoiceDbContext : AbpDbContext<InvoiceDbContext>
{
    public DbSet<Invoice> Invoices { get; set; }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        builder.ConfigureInvoice();
    }
}

// ShipMvp.Invoice.EntityFrameworkCore/Repositories/InvoiceRepository.cs
public class EfCoreInvoiceRepository : EfCoreRepository<InvoiceDbContext, Invoice, Guid>, IInvoiceRepository
{
    public EfCoreInvoiceRepository(IDbContextProvider<InvoiceDbContext> dbContextProvider)
        : base(dbContextProvider)
    {
    }
}
```

#### Checklist:

- [ ] Create ABP-compliant DbContext
- [ ] Implement repository using ABP's EfCoreRepository base
- [ ] Configure entity mappings with FluentAPI
- [ ] Add connection string configuration
- [ ] Implement database migrations
- [ ] Configure Unit of Work pattern

## Phase 4: HTTP API Layer (Effort: Small, 0.5 days)

### 4.1 Create REST Controllers

```csharp
// ShipMvp.Invoice.HttpApi/Controllers/InvoiceController.cs
[Route("api/invoices")]
public class InvoiceController : AbpControllerBase, IInvoiceAppService
{
    private readonly IInvoiceAppService _invoiceAppService;

    [HttpPost]
    public Task<InvoiceDto> CreateAsync(CreateInvoiceDto input)
    {
        return _invoiceAppService.CreateAsync(input);
    }

    [HttpGet("{id}")]
    public Task<InvoiceDto> GetAsync(Guid id)
    {
        return _invoiceAppService.GetAsync(id);
    }
}
```

#### Checklist:

- [ ] Create controllers implementing application service interfaces
- [ ] Add proper HTTP method attributes and routing
- [ ] Configure Swagger/OpenAPI documentation
- [ ] Add API versioning if needed
- [ ] Implement proper HTTP status codes
- [ ] Add authorization attributes where required

## Phase 5: Testing & Validation (Effort: Small, 0.5 days)

### 5.1 Build Verification

#### Checklist:

- [ ] Ensure all projects build without errors
- [ ] Verify proper dependency injection registration
- [ ] Test API endpoints with Swagger UI
- [ ] Validate database operations
- [ ] Run existing unit tests (if any)
- [ ] Perform integration testing

### 5.2 Documentation Updates

#### Checklist:

- [ ] Update README.md with new project structure
- [ ] Document API endpoints and DTOs
- [ ] Create developer setup guide
- [ ] Update architectural decision records (ADRs)

## Rollback Strategy

If issues arise during migration:

1. **Phase-by-phase rollback:** Each phase can be reverted independently
2. **Feature flags:** Use configuration to toggle between old/new implementations
3. **Database compatibility:** Maintain backward-compatible schema changes
4. **API versioning:** Keep existing endpoints while adding new ones

## Post-Migration Benefits

- **ABP Ecosystem:** Access to pre-built modules and features
- **Scalability:** Proper microservice decomposition support
- **Maintainability:** Clear layer separation and dependency rules
- **Developer Experience:** Standardized patterns and conventions
- **Testing:** Improved testability with proper abstractions

## Success Criteria

- ✅ All projects build successfully
- ✅ API endpoints return correct responses
- ✅ Database operations work correctly
- ✅ Swagger documentation is generated
- ✅ No breaking changes to existing functionality
- ✅ Code follows ABP naming conventions
- ✅ Proper dependency injection configuration
