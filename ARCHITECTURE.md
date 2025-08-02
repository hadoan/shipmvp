# ShipMvp Architecture Documentation

## Table of Contents
- [Overview](#overview)
- [Architecture Principles](#architecture-principles)
- [Project Structure](#project-structure)
- [Module Details](#module-details)
- [Data Layer](#data-layer)
- [Domain Layer](#domain-layer)
- [Application Layer](#application-layer)
- [Infrastructure Layer](#infrastructure-layer)
- [API Layer](#api-layer)
- [Host Layer](#host-layer)
- [Frontend Architecture](#frontend-architecture)
- [Cross-Cutting Concerns](#cross-cutting-concerns)
- [Development Guidelines](#development-guidelines)

---

## Overview

ShipMvp is a modular .NET application built using Clean Architecture principles with a React TypeScript frontend. The application implements a subscription-based invoice management system with Stripe payment integration.

### Key Technologies
- **Backend**: .NET 9.0, Entity Framework Core, SQLite
- **Frontend**: React 18, TypeScript, Vite, TailwindCSS
- **Payment**: Stripe API
- **Database**: SQLite (Development), configurable for production
- **Architecture**: Modular Monolith with Clean Architecture

---

## Architecture Principles

### 1. Separation of Concerns
Each module has a single, well-defined responsibility:
- **Domain**: Business logic and rules
- **Application**: Use cases and orchestration
- **Infrastructure**: External integrations and persistence
- **API**: HTTP endpoints and request/response handling
- **Host**: Application bootstrapping and configuration

### 2. Dependency Inversion
- High-level modules don't depend on low-level modules
- Both depend on abstractions (interfaces)
- Infrastructure implements domain interfaces

### 3. Modularity
- Each module is independently testable
- Clear boundaries between modules
- Dependencies are explicit through module attributes

### 4. Testability
- Dependency injection throughout
- Interface-based design
- Repository pattern for data access

---

## Project Structure

```
backend/
├── src/
│   ├── ShipMvp.Shared/           # Common types and interfaces
│   ├── ShipMvp.Domain/           # Business entities and domain logic
│   ├── ShipMvp.Application/      # Use cases and application services
│   ├── ShipMvp.Infrastructure/   # Data access and external integrations
│   ├── ShipMvp.Api/             # HTTP API controllers and DTOs
│   └── ShipMvp.Host/            # Application entry point and configuration
└── tests/                       # Unit and integration tests

frontend/
├── src/
│   ├── components/              # Reusable UI components
│   ├── pages/                   # Page components
│   ├── lib/                     # Utilities and services
│   ├── hooks/                   # Custom React hooks
│   └── types/                   # TypeScript type definitions
└── public/                      # Static assets
```

---

## Module Details

### Module Dependency Graph
```
Host Module
    ├── API Module
    │   └── Application Module
    │       └── Domain Module
    │           └── Shared Module
    └── Infrastructure Module
        ├── Domain Module
        └── Shared Module
```

### Module Attributes
Each module uses the `[Module]` attribute and `[DependsOn<T>]` to define dependencies:

```csharp
[Module]
[DependsOn<ApiModule>]
public class HostModule : IModule
```

---

## Data Layer

### Database Context (`AppDbContext`)

**Location**: `ShipMvp.Infrastructure/Data/DataLayer.cs`

The `AppDbContext` serves as the primary data access layer:

```csharp
public class AppDbContext : DbContext
{
    // Entity Sets
    public DbSet<Invoice> Invoices { get; set; }
    public DbSet<User> Users { get; set; }
    public DbSet<SubscriptionPlan> SubscriptionPlans { get; set; }
    public DbSet<UserSubscription> UserSubscriptions { get; set; }
    public DbSet<SubscriptionUsage> SubscriptionUsage { get; set; }

    // Configuration
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Entity configurations
        ConfigureInvoiceEntity(modelBuilder);
        ConfigureSubscriptionEntities(modelBuilder);
        ConfigureUserEntity(modelBuilder);
    }
}
```

#### Key Features:
- **Entity Configuration**: Fluent API configurations for complex relationships
- **Owned Types**: `InvoiceItem` and `Money` value objects
- **Database Provider**: SQLite for development, configurable for production
- **Migrations**: EF Core migrations for schema versioning

#### Database Configuration:

The application supports multiple database providers through Entity Framework Core:

```csharp
// SQLite (Development/Local)
services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite(connectionString));

// PostgreSQL (Production Recommended)
services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(connectionString, npgsqlOptions =>
    {
        npgsqlOptions.EnableRetryOnFailure(
            maxRetryCount: 3,
            maxRetryDelay: TimeSpan.FromSeconds(30),
            errorCodesToAdd: null);
    }));

// SQL Server
services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(connectionString, sqlOptions =>
    {
        sqlOptions.EnableRetryOnFailure(
            maxRetryCount: 3,
            maxRetryDelay: TimeSpan.FromSeconds(30),
            errorNumbersToAdd: null);
        sqlOptions.CommandTimeout(30);
    }));

// MySQL
services.AddDbContext<AppDbContext>(options =>
    options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString), mysqlOptions =>
    {
        mysqlOptions.EnableRetryOnFailure(
            maxRetryCount: 3,
            maxRetryDelay: TimeSpan.FromSeconds(30),
            errorNumbersToAdd: null);
    }));

// In-Memory (Testing)
services.AddDbContext<AppDbContext>(options =>
    options.UseInMemoryDatabase("ShipMvpDb"));
```

#### Connection String Examples:

```json
{
  "ConnectionStrings": {
    // SQLite
    "DefaultConnection": "Data Source=shipmvp.db",

    // PostgreSQL
    "PostgreSQL": "Host=localhost;Database=shipmvp;Username=postgres;Password=yourpassword;Port=5432;Pooling=true;Minimum Pool Size=0;Maximum Pool Size=100;",

    // SQL Server
    "SqlServer": "Server=localhost;Database=ShipMvp;Integrated Security=true;TrustServerCertificate=true;MultipleActiveResultSets=true;",

    // SQL Server with authentication
    "SqlServerAuth": "Server=localhost;Database=ShipMvp;User Id=sa;Password=YourStrong@Passw0rd;TrustServerCertificate=true;MultipleActiveResultSets=true;",

    // MySQL
    "MySQL": "Server=localhost;Database=shipmvp;Uid=root;Pwd=yourpassword;Port=3306;SslMode=none;",

    // Azure SQL Database
    "AzureSQL": "Server=tcp:yourserver.database.windows.net,1433;Initial Catalog=ShipMvp;Persist Security Info=False;User ID=yourusername;Password=yourpassword;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;",

    // Azure PostgreSQL
    "AzurePostgreSQL": "Host=yourserver.postgres.database.azure.com;Database=shipmvp;Username=yourusername@yourserver;Password=yourpassword;Port=5432;SslMode=Require;"
  }
}
```

#### Environment-Specific Configuration:

```csharp
public void ConfigureServices(IServiceCollection services)
{
    var connectionString = configuration.GetConnectionString("DefaultConnection");
    var databaseProvider = configuration["DatabaseProvider"] ?? "SQLite";

    services.AddDbContext<AppDbContext>(options =>
    {
        switch (databaseProvider.ToUpperInvariant())
        {
            case "POSTGRESQL":
                options.UseNpgsql(connectionString, npgsqlOptions =>
                {
                    npgsqlOptions.EnableRetryOnFailure(3, TimeSpan.FromSeconds(30), null);
                    npgsqlOptions.MigrationsAssembly("ShipMvp.Infrastructure");
                });
                break;

            case "SQLSERVER":
                options.UseSqlServer(connectionString, sqlOptions =>
                {
                    sqlOptions.EnableRetryOnFailure(3, TimeSpan.FromSeconds(30), null);
                    sqlOptions.MigrationsAssembly("ShipMvp.Infrastructure");
                });
                break;

            case "MYSQL":
                options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString), mysqlOptions =>
                {
                    mysqlOptions.EnableRetryOnFailure(3, TimeSpan.FromSeconds(30), null);
                    mysqlOptions.MigrationsAssembly("ShipMvp.Infrastructure");
                });
                break;

            case "SQLITE":
            default:
                options.UseSqlite(connectionString, sqliteOptions =>
                {
                    sqliteOptions.MigrationsAssembly("ShipMvp.Infrastructure");
                });
                break;
        }

        if (environment.IsDevelopment())
        {
            options.EnableSensitiveDataLogging();
            options.EnableDetailedErrors();
        }
    });
}
```

#### Required NuGet Packages:

```xml
<!-- SQLite (Default) -->
<PackageReference Include="Microsoft.EntityFrameworkCore.Sqlite" Version="9.0.0" />

<!-- PostgreSQL -->
<PackageReference Include="Npgsql.EntityFrameworkCore.PostgreSQL" Version="9.0.0" />

<!-- SQL Server -->
<PackageReference Include="Microsoft.EntityFrameworkCore.SqlServer" Version="9.0.0" />

<!-- MySQL -->
<PackageReference Include="Pomelo.EntityFrameworkCore.MySql" Version="9.0.0" />

<!-- In-Memory (Testing) -->
<PackageReference Include="Microsoft.EntityFrameworkCore.InMemory" Version="9.0.0" />
```

---

## Domain Layer

### Entities and Aggregates

**Location**: `ShipMvp.Domain/`

#### Invoice Aggregate
```csharp
public sealed record Invoice(
    Guid Id,
    string CustomerName,
    Money TotalAmount,
    DateTime CreatedAt,
    InvoiceStatus Status
)
{
    public IReadOnlyList<InvoiceItem> Items { get; init; } = new List<InvoiceItem>();
}

public sealed record InvoiceItem(Guid Id, string Description, Money Amount);
```

#### Subscription Entities
```csharp
public class SubscriptionPlan
{
    public string Id { get; set; }
    public string Name { get; set; }
    public decimal Price { get; set; }
    public int MaxInvoices { get; set; }
    public int MaxUsers { get; set; }
    // ... other properties
}

public class UserSubscription
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string PlanId { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public SubscriptionStatus Status { get; set; }
    // ... stripe integration properties
}
```

#### Value Objects
```csharp
public sealed record Money(decimal Amount, string Currency = "USD")
{
    public static Money Zero => new(0);
    public static Money Create(decimal amount, string currency = "USD")
        => new(amount, currency);
}
```

### Domain Services

**Location**: `ShipMvp.Domain/Invoices/Invoice.cs`

```csharp
public interface IInvoiceDomainService
{
    Invoice CreateInvoice(string customerName, IEnumerable<(string Description, decimal Amount)> items);
}

public class InvoiceDomainService : IInvoiceDomainService
{
    private readonly IGuidGenerator _guidGenerator;

    public Invoice CreateInvoice(string customerName, IEnumerable<(string Description, decimal Amount)> items)
    {
        var invoiceId = _guidGenerator.Create();
        var invoiceItems = items.Select(item =>
            InvoiceItem.Create(item.Description, item.Amount, "USD", _guidGenerator)).ToList();

        return new Invoice(invoiceId, customerName, invoiceItems);
    }
}
```

---

## Application Layer

### Application Services

**Location**: `ShipMvp.Application/`

#### Invoice Service
```csharp
public interface IInvoiceService
{
    Task<Invoice> CreateInvoiceAsync(CreateInvoiceRequest request);
    Task<Invoice?> GetInvoiceAsync(Guid id);
    Task<IEnumerable<Invoice>> GetInvoicesAsync();
    Task DeleteInvoiceAsync(Guid id);
}

public class InvoiceService : IInvoiceService
{
    private readonly IInvoiceRepository _invoiceRepository;
    private readonly IInvoiceDomainService _domainService;
    private readonly IUnitOfWork _unitOfWork;

    public async Task<Invoice> CreateInvoiceAsync(CreateInvoiceRequest request)
    {
        var invoice = _domainService.CreateInvoice(
            request.CustomerName,
            request.Items.Select(i => (i.Description, i.Amount))
        );

        await _invoiceRepository.AddAsync(invoice);
        await _unitOfWork.SaveChangesAsync();

        return invoice;
    }
}
```

#### Subscription Service
```csharp
public interface ISubscriptionService
{
    Task<UserSubscription> CreateSubscriptionAsync(Guid userId, string planId);
    Task<UserSubscription?> GetActiveSubscriptionAsync(Guid userId);
    Task<bool> CheckUsageLimitAsync(Guid userId, string feature);
}
```

### DTOs and Requests
```csharp
public record CreateInvoiceRequest(
    string CustomerName,
    IEnumerable<CreateInvoiceItemRequest> Items
);

public record CreateInvoiceItemRequest(
    string Description,
    decimal Amount
);
```

---

## Infrastructure Layer

### Repository Implementations

**Location**: `ShipMvp.Infrastructure/Repositories/`

#### Invoice Repository
```csharp
public interface IInvoiceRepository
{
    Task<Invoice?> GetByIdAsync(Guid id);
    Task<IEnumerable<Invoice>> GetAllAsync();
    Task AddAsync(Invoice invoice);
    Task DeleteAsync(Guid id);
}

public class InvoiceRepository : IInvoiceRepository
{
    private readonly AppDbContext _context;

    public async Task<Invoice?> GetByIdAsync(Guid id)
    {
        return await _context.Invoices
            .Include(i => i.Items)
            .FirstOrDefaultAsync(i => i.Id == id);
    }

    public async Task AddAsync(Invoice invoice)
    {
        await _context.Invoices.AddAsync(invoice);
    }
}
```

#### Unit of Work Pattern
```csharp
public interface IUnitOfWork
{
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}

public class EfUnitOfWork : IUnitOfWork
{
    private readonly AppDbContext _context;

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return await _context.SaveChangesAsync(cancellationToken);
    }
}
```

### External Integrations

#### Stripe Service
```csharp
public interface IStripeService
{
    Task<string> CreateCheckoutSessionAsync(string priceId, Guid userId);
    Task<bool> ValidateWebhookAsync(string payload, string signature);
    Task HandleWebhookEventAsync(string eventType, object eventData);
}

public class StripeService : IStripeService
{
    private readonly StripeClient _stripeClient;
    private readonly IConfiguration _configuration;

    public async Task<string> CreateCheckoutSessionAsync(string priceId, Guid userId)
    {
        var service = new SessionService(_stripeClient);
        var options = new SessionCreateOptions
        {
            PaymentMethodTypes = new List<string> { "card" },
            LineItems = new List<SessionLineItemOptions>
            {
                new() { Price = priceId, Quantity = 1 }
            },
            Mode = "subscription",
            SuccessUrl = "https://yourapp.com/success",
            CancelUrl = "https://yourapp.com/cancel",
            Metadata = new Dictionary<string, string> { { "user_id", userId.ToString() } }
        };

        var session = await service.CreateAsync(options);
        return session.Url;
    }
}
```

### Background Jobs (Future Enhancement)

```csharp
public interface IBackgroundJobService
{
    Task ScheduleAsync<T>(Expression<Func<T, Task>> methodCall, TimeSpan delay);
    Task EnqueueAsync<T>(Expression<Func<T, Task>> methodCall);
}

// Example usage for subscription renewals
public class SubscriptionRenewalJob
{
    public async Task ProcessRenewalAsync(Guid subscriptionId)
    {
        // Process subscription renewal logic
    }
}
```

### Dependency Injection Configuration

**Location**: `ShipMvp.Infrastructure/InfrastructureModule.cs`

```csharp
[Module]
public class InfrastructureModule : IModule
{
    public void ConfigureServices(IServiceCollection services)
    {
        // Database
        services.AddDbContext<AppDbContext>(options =>
        {
            if (environment.IsDevelopment())
            {
                options.UseSqlite(connectionString);
            }
            else
            {
                options.UseInMemoryDatabase("ShipMvpDb");
            }
        });

        // Repositories
        services.AddScoped<IInvoiceRepository, InvoiceRepository>();
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IUnitOfWork, EfUnitOfWork>();

        // Domain services and utilities
        services.AddSingleton<IGuidGenerator, SequentialGuidGenerator>();
        services.AddScoped<IInvoiceDomainService, InvoiceDomainService>();

        // Subscription repositories
        services.AddScoped<ISubscriptionPlanRepository, SubscriptionPlanRepository>();
        services.AddScoped<IUserSubscriptionRepository, UserSubscriptionRepository>();
        services.AddScoped<ISubscriptionUsageRepository, SubscriptionUsageRepository>();

        // External services
        services.AddScoped<IStripeService, StripeService>();
    }

    public void Configure(IApplicationBuilder app, IHostEnvironment env)
    {
        // Ensure database is created and apply migrations
        using var scope = app.ApplicationServices.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        if (env.IsDevelopment())
        {
            context.Database.Migrate();
        }
        else
        {
            context.Database.EnsureCreated();
        }

        // Seed initial data
        DataSeeder.SeedAsync(context).GetAwaiter().GetResult();
    }
}
```

---

## API Layer

### Controllers

**Location**: `ShipMvp.Api/Controllers/`

```csharp
[ApiController]
[Route("api/[controller]")]
public class InvoicesController : ControllerBase
{
    private readonly IInvoiceService _invoiceService;

    [HttpGet]
    public async Task<ActionResult<IEnumerable<Invoice>>> GetInvoices()
    {
        var invoices = await _invoiceService.GetInvoicesAsync();
        return Ok(invoices);
    }

    [HttpPost]
    public async Task<ActionResult<Invoice>> CreateInvoice(CreateInvoiceRequest request)
    {
        var invoice = await _invoiceService.CreateInvoiceAsync(request);
        return CreatedAtAction(nameof(GetInvoice), new { id = invoice.Id }, invoice);
    }
}
```

### API Module Configuration

**Location**: `ShipMvp.Api/ApiModule.cs`

```csharp
[Module]
[DependsOn<ApplicationModule>]
public class ApiModule : IModule
{
    public void ConfigureServices(IServiceCollection services)
    {
        // Controllers
        services.AddControllers();

        // API Documentation
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen();

        // API-specific middleware
        services.AddProblemDetails();
    }

    public void Configure(IApplicationBuilder app, IHostEnvironment env)
    {
        if (env.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }

        app.UseRouting();
        app.UseAuthentication();
        app.UseAuthorization();
        app.MapControllers();
    }
}
```

---

## Host Layer

### Application Bootstrap

**Location**: `ShipMvp.Host/HostModule.cs`

```csharp
[Module]
[DependsOn<ApiModule>]
public class HostModule : IModule
{
    public void ConfigureServices(IServiceCollection services)
    {
        // Enhanced CORS configuration
        services.AddCors();
    }

    public void Configure(IApplicationBuilder app, IHostEnvironment env)
    {
        var logger = app.ApplicationServices.GetRequiredService<ILogger<HostModule>>();
        var configuration = app.ApplicationServices.GetRequiredService<IConfiguration>();

        // Configure CORS policy at runtime
        ConfigureCorsPolicy(app, configuration, logger);

        if (env.IsDevelopment())
        {
            logger.LogInformation("Host: Development environment detected. Enhanced logging enabled.");
        }
    }
}
```

### CORS Configuration
The host module implements sophisticated CORS handling:
- Reads from `appsettings.json` and environment variables
- Supports wildcard patterns (e.g., `https://*.vercel.app`)
- Logs configuration for debugging
- Falls back to development defaults if no origins configured

### Startup Process
1. **Service Registration**: All modules register their services
2. **Configuration**: Modules configure their middleware and settings
3. **Database**: Migrations applied, initial data seeded
4. **Logging**: Comprehensive startup logging for debugging

---

## Frontend Architecture

### Technology Stack
- **React 18** with TypeScript
- **Vite** for build tooling
- **TailwindCSS** for styling
- **Shadcn/ui** for component library
- **React Query** for server state management
- **React Hook Form** for form handling
- **Stripe Elements** for payment processing

### Project Structure
```
frontend/src/
├── components/
│   ├── ui/                    # Shadcn/ui components
│   ├── AppHeader.tsx          # Application header
│   ├── Sidebar.tsx            # Navigation sidebar
│   └── ThemeProvider.tsx      # Theme context
├── pages/
│   ├── Index.tsx              # Dashboard
│   ├── Invoices.tsx           # Invoice management
│   ├── BillingPage.tsx        # Subscription management
│   └── LoginPage.tsx          # Authentication
├── lib/
│   ├── api/                   # API service layer
│   ├── auth/                  # Authentication context
│   ├── subscription/          # Subscription context
│   └── utils.ts               # Utility functions
└── hooks/                     # Custom React hooks
```

### API Integration
```typescript
// API Client Configuration
export const apiClient = ofetch.create({
  baseURL: config.apiUrl,
  credentials: 'include',
  headers: {
    'Content-Type': 'application/json',
  },
});

// Type-safe API calls
export const invoiceService = {
  async getInvoices(): Promise<Invoice[]> {
    return apiClient('/api/invoices');
  },

  async createInvoice(invoice: CreateInvoiceRequest): Promise<Invoice> {
    return apiClient('/api/invoices', {
      method: 'POST',
      body: invoice,
    });
  },
};
```

### State Management
- **React Query**: Server state, caching, and synchronization
- **React Context**: Application-wide state (auth, subscription)
- **React Hook Form**: Form state and validation

---

## Cross-Cutting Concerns

### GUID Generation
**Location**: `ShipMvp.Shared/IGuidGenerator.cs`

Sequential GUID generation for better database performance:
```csharp
public interface IGuidGenerator
{
    Guid Create();
}

public class SequentialGuidGenerator : IGuidGenerator
{
    public Guid Create()
    {
        // Creates GUIDs that are sequential for database indexing
        return CreateSequentialGuid();
    }
}
```

### Error Handling
- **Domain**: Custom exceptions for business rule violations
- **Application**: Result patterns for operation outcomes
- **API**: Problem Details for standardized error responses
- **Infrastructure**: Logging and resilience patterns

### Logging
- **Structured logging** with Serilog
- **Context-aware** logging with correlation IDs
- **Performance metrics** for database operations
- **Security audit** logging for sensitive operations

### Configuration
- **Hierarchical configuration**: appsettings.json → environment variables → command line
- **Environment-specific** settings
- **Feature flags** for conditional functionality
- **Connection string** management

---

## Development Guidelines

### Code Organization
1. **Domain First**: Start with domain entities and business rules
2. **Interface Segregation**: Define interfaces before implementations
3. **Single Responsibility**: Each class has one reason to change
4. **Dependency Injection**: Use constructor injection throughout

### Testing Strategy
```csharp
// Unit Tests
[Test]
public void CreateInvoice_ShouldGenerateSequentialId()
{
    // Arrange
    var guidGenerator = new SequentialGuidGenerator();
    var domainService = new InvoiceDomainService(guidGenerator);

    // Act
    var invoice = domainService.CreateInvoice("Test Customer", items);

    // Assert
    Assert.That(invoice.Id, Is.Not.EqualTo(Guid.Empty));
}

// Integration Tests
[Test]
public async Task CreateInvoice_ShouldPersistToDatabase()
{
    // Arrange
    using var context = CreateTestContext();
    var repository = new InvoiceRepository(context);

    // Act & Assert
    // Test full flow through repository
}
```

### Database Migrations
```bash
# Create migration
dotnet ef migrations add MigrationName --project src/ShipMvp.Infrastructure --startup-project src/ShipMvp.Host

# Apply migration
dotnet ef database update --project src/ShipMvp.Infrastructure --startup-project src/ShipMvp.Host

# Generate SQL script
dotnet ef migrations script --project src/ShipMvp.Infrastructure --startup-project src/ShipMvp.Host
```

### Performance Considerations
1. **Database Queries**: Use projection and avoid N+1 problems
2. **Caching**: Implement caching for frequently accessed data
3. **Background Jobs**: Move long-running operations to background
4. **Connection Pooling**: Configure EF Core connection pooling

### Security Best Practices
1. **Input Validation**: Validate all user inputs
2. **Authorization**: Implement proper role-based access control
3. **Data Protection**: Encrypt sensitive data at rest
4. **Audit Logging**: Log all significant business operations

---

## Future Enhancements

### Planned Features
1. **Multi-tenancy**: Support for multiple organizations
2. **Background Jobs**: Hangfire integration for scheduled tasks
3. **Caching**: Redis integration for performance
4. **Monitoring**: Application insights and health checks
5. **Authentication**: Identity Server integration
6. **File Storage**: Azure Blob Storage for attachments

### Scalability Considerations
1. **Microservices**: Extract bounded contexts into separate services
2. **Event Sourcing**: Implement for audit trails and replaying events
3. **CQRS**: Separate read and write models for complex queries
4. **Message Queues**: RabbitMQ or Azure Service Bus for async processing

---

This architecture provides a solid foundation for a maintainable, testable, and scalable application while following industry best practices and clean architecture principles.
