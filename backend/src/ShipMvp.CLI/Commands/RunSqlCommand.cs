using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ShipMvp.Core.Persistence;
using ShipMvp.Domain.Integrations;
using ShipMvp.Domain.Identity;
using System.Data;
using System.Reflection;
using System.Dynamic;

namespace ShipMvp.CLI.Commands;

public class RunSqlCommand : ICommand
{
    private readonly IDbContext _dbContext;
    private readonly ILogger<RunSqlCommand> _logger;

    public RunSqlCommand(IDbContext dbContext, ILogger<RunSqlCommand> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    public async Task ExecuteAsync(string[] args, CancellationToken cancellationToken = default)
    {
        if (args.Length == 0)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("❌ SQL query is required.");
            Console.ResetColor();
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("Usage: dotnet run run-sql \"SELECT * FROM \"Integrations\"\"");
            Console.WriteLine("Usage: dotnet run run-sql \"DELETE FROM \"Integrations\"\"");
            Console.WriteLine("Usage: dotnet run run-sql \"UPDATE \"Integrations\" SET \"Name\" = 'New Name'\"");
            Console.WriteLine("Available tables: \"Integrations\", \"Users\", \"SubscriptionPlans\", etc.");
            Console.ResetColor();
            return;
        }

        var sqlQuery = string.Join(" ", args);
        
        try
        {
            Console.ForegroundColor = ConsoleColor.Blue;
            Console.WriteLine($"🔍 Executing query: {sqlQuery}");
            Console.ResetColor();
            
            // Determine query type
            var queryType = DetermineQueryType(sqlQuery);
            
            switch (queryType)
            {
                case QueryType.Select:
                    await ExecuteSelectQuery(sqlQuery, cancellationToken);
                    break;
                case QueryType.Delete:
                    await ExecuteDeleteQuery(sqlQuery, cancellationToken);
                    break;
                case QueryType.Update:
                    await ExecuteUpdateQuery(sqlQuery, cancellationToken);
                    break;
                case QueryType.Insert:
                    await ExecuteInsertQuery(sqlQuery, cancellationToken);
                    break;
                default:
                    await ExecuteGenericQuery(sqlQuery, cancellationToken);
                    break;
            }
        }
        catch (Exception ex)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"❌ Error executing query: {ex.Message}");
            Console.ResetColor();
            _logger.LogError(ex, "Error executing query: {Query}", sqlQuery);
            throw;
        }
    }

    private QueryType DetermineQueryType(string sqlQuery)
    {
        var upperQuery = sqlQuery.Trim().ToUpperInvariant();
        
        if (upperQuery.StartsWith("SELECT"))
            return QueryType.Select;
        if (upperQuery.StartsWith("DELETE"))
            return QueryType.Delete;
        if (upperQuery.StartsWith("UPDATE"))
            return QueryType.Update;
        if (upperQuery.StartsWith("INSERT"))
            return QueryType.Insert;
        
        return QueryType.Unknown;
    }

    private async Task ExecuteSelectQuery(string sqlQuery, CancellationToken cancellationToken)
    {
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine("📊 Executing SELECT query...");
        Console.ResetColor();

        var result = await Task.Run(() => DynamicListFromSql((DbContext)_dbContext, sqlQuery, new Dictionary<string, object>()).ToList(), cancellationToken);
        
        Console.WriteLine();
        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine($"✓ Query executed successfully. Found {result.Count} row(s).");
        Console.ResetColor();
        
        if (result.Any())
        {
            Console.WriteLine();
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("Results (first 10 rows):");
            Console.WriteLine("=======================");
            Console.ResetColor();
            
            var count = 0;
            foreach (var row in result.Take(10))
            {
                Console.WriteLine($"Row {++count}:");
                foreach (var property in (IDictionary<string, object>)row)
                {
                    Console.WriteLine($"  {property.Key}: {property.Value}");
                }
                Console.WriteLine();
            }
            
            if (result.Count > 10)
            {
                Console.ForegroundColor = ConsoleColor.Gray;
                Console.WriteLine($"... and {result.Count - 10} more rows.");
                Console.ResetColor();
            }
        }
    }

    private async Task ExecuteDeleteQuery(string sqlQuery, CancellationToken cancellationToken)
    {
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine("🗑️  Executing DELETE query...");
        Console.ResetColor();

        // Show confirmation for destructive operations
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine("⚠️  WARNING: This is a DELETE operation!");
        Console.WriteLine($"Query: {sqlQuery}");
        Console.Write("Are you sure you want to continue? (y/N): ");
        Console.ResetColor();
        
        var response = Console.ReadLine()?.Trim().ToLowerInvariant();
        if (response != "y" && response != "yes")
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("❌ Operation cancelled by user.");
            Console.ResetColor();
            return;
        }

        var rowsAffected = await _dbContext.Database.ExecuteSqlRawAsync(sqlQuery, cancellationToken);
        
        Console.WriteLine();
        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine($"✓ DELETE query executed successfully. {rowsAffected} row(s) affected.");
        Console.ResetColor();
    }

    private async Task ExecuteUpdateQuery(string sqlQuery, CancellationToken cancellationToken)
    {
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine("✏️  Executing UPDATE query...");
        Console.ResetColor();

        // Show confirmation for destructive operations
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine("⚠️  WARNING: This is an UPDATE operation!");
        Console.WriteLine($"Query: {sqlQuery}");
        Console.Write("Are you sure you want to continue? (y/N): ");
        Console.ResetColor();
        
        var response = Console.ReadLine()?.Trim().ToLowerInvariant();
        if (response != "y" && response != "yes")
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("❌ Operation cancelled by user.");
            Console.ResetColor();
            return;
        }

        var rowsAffected = await _dbContext.Database.ExecuteSqlRawAsync(sqlQuery, cancellationToken);
        
        Console.WriteLine();
        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine($"✓ UPDATE query executed successfully. {rowsAffected} row(s) affected.");
        Console.ResetColor();
    }

    private async Task ExecuteInsertQuery(string sqlQuery, CancellationToken cancellationToken)
    {
        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine("➕ Executing INSERT query...");
        Console.ResetColor();

        var rowsAffected = await _dbContext.Database.ExecuteSqlRawAsync(sqlQuery, cancellationToken);
        
        Console.WriteLine();
        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine($"✓ INSERT query executed successfully. {rowsAffected} row(s) affected.");
        Console.ResetColor();
    }

    private async Task ExecuteGenericQuery(string sqlQuery, CancellationToken cancellationToken)
    {
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine("🔧 Executing generic query...");
        Console.ResetColor();

        try
        {
            var rowsAffected = await _dbContext.Database.ExecuteSqlRawAsync(sqlQuery, cancellationToken);
            
            Console.WriteLine();
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"✓ Query executed successfully. {rowsAffected} row(s) affected.");
            Console.ResetColor();
        }
        catch (Exception ex)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"❌ Error executing generic query: {ex.Message}");
            Console.ResetColor();
            throw;
        }
    }

    public async Task ExecuteAsync(CancellationToken cancellationToken = default)
    {
        await ExecuteAsync(new string[0], cancellationToken);
    }

    private static IEnumerable<dynamic> DynamicListFromSql(DbContext db, string sql, Dictionary<string, object> parameters)
    {
        var connection = db.Database.GetDbConnection();
        if (connection == null)
        {
            throw new InvalidOperationException("Database connection is not available");
        }

        using (var cmd = connection.CreateCommand())
        {
            cmd.CommandText = sql;
            if (cmd.Connection.State != ConnectionState.Open) { cmd.Connection.Open(); }

            foreach (KeyValuePair<string, object> p in parameters)
            {
                var dbParameter = cmd.CreateParameter();
                dbParameter.ParameterName = p.Key;
                dbParameter.Value = p.Value;
                cmd.Parameters.Add(dbParameter);
            }

            using (var dataReader = cmd.ExecuteReader())
            {
                while (dataReader.Read())
                {
                    var row = new ExpandoObject() as IDictionary<string, object>;
                    for (var fieldCount = 0; fieldCount < dataReader.FieldCount; fieldCount++)
                    {
                        row.Add(dataReader.GetName(fieldCount), dataReader[fieldCount]);
                    }
                    yield return row;
                }
            }
        }
    }
}

public enum QueryType
{
    Unknown,
    Select,
    Delete,
    Update,
    Insert
}
