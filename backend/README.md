# ShipMvp

Ultra-lean .NET 8 SaaS backend template.

## Run

```
dotnet run --project src/ShipMvp.Host
```

## Try the API

- Open http://localhost:5000/swagger
- Or, create an invoice:

```
curl -X POST http://localhost:5000/api/invoices -H "Content-Type: application/json" -d '{"customerName":"Acme","items":[{"description":"Widget","amount":100}]}'
```

- Fetch an invoice:

```
curl http://localhost:5000/api/invoices/{id}
```

---

Frontend lives in `/frontend`.

---

## How to Add a New Model (DDD Style)

1. **Create Core Domain Objects**

   - In `src/Modules/<YourModule>/<YourModule>.Core/`, add your entity (record/class) implementing `IEntity<TKey>` or inheriting `AggregateRoot<TKey>`.
   - Define value objects as `record` types if needed.
   - Example:
     ```csharp
     public sealed record Product(Guid Id, string Name) : AggregateRoot<Guid>(Id);
     ```

2. **Define Repository Interface**

   - In the same Core folder, add an interface extending `IRepository<T, TKey>`.
   - Example:
     ```csharp
     public interface IProductRepository : IRepository<Product, Guid> {}
     ```

3. **Add Application Logic**

   - Add MediatR commands/queries and handlers in Core (e.g., `CreateProductCommand`, `CreateProductHandler`).

4. **Implement Infrastructure**

   - In `src/Modules/<YourModule>/<YourModule>.Infrastructure/`, implement your repository (e.g., `ProductRepository : EfRepository<Product, Guid>, IProductRepository`).
   - Register your DbContext and repository in the module's `ConfigureServices`.

5. **Expose Endpoints**

   - In your module's `MapEndpoints`, add minimal API routes for your model.

6. **Register the Module**
   - Ensure your module is registered in `Program.cs` via `AddAppModules`.

---

**Tip:** Keep each file <150 LOC, use records for immutability, and keep modules independent.
