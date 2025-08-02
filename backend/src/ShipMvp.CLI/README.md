# ShipMvp CLI Tool

## Usage

Run the CLI with:

```bash
dotnet run -- <command> [options]
```

## Available Commands

- `seed-integrations` Seed integration platforms from appsettings.json
- `seed-data` Seed initial application data (users, plans, etc.)
- `run-sql` Execute a SQL query against the database (supports some basic queries)
- `help` Show help message

## Examples

```bash
# Seed integrations
cd backend/src/ShipMvp.CLI
dotnet run -- seed-integrations

# Seed initial data
dotnet run -- seed-data

# Run a SQL query (see below for supported queries)
dotnet run -- run-sql "SELECT * FROM Integrations"
dotnet run -- run-sql "SELECT * FROM Users"
```

---

# SQL Query Examples

The `run-sql` command currently supports some basic queries using LINQ for compatibility:

- List all integration platforms:
  ```bash
  dotnet run -- run-sql "SELECT * FROM Integrations"
  ```
- List first 10 users:
  ```bash
  dotnet run -- run-sql "SELECT * FROM Users"
  ```

> **Note:**
>
> - Generic SQL queries are not supported yet. Only the above queries are available.
> - Output is colorized for readability.

---

For more details, see the inline help:

```bash
dotnet run -- help
```
