using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Reflection;

namespace ShipMvp.Application.Infrastructure.Data;

public static class ModelBuilderExtensions
{
    /// <summary>
    /// Automatically configures the ExtraProperties dictionary for every entity that defines it,
    /// mapping it to a PostgreSQL jsonb column named "ExtraProperties" (or keeping any explicit config).
    /// </summary>
    public static void MapExtraPropertiesAsJson(this ModelBuilder modelBuilder)
    {
        foreach (var entity in modelBuilder.Model.GetEntityTypes())
        {
            var clrType = entity.ClrType;
            if (clrType == null || clrType == typeof(object)) continue;

            // Skip if ExtraProperties already configured (e.g., specific entity mapping)
            if (entity.FindProperty("ExtraProperties") != null) continue;

            // Skip value objects and types that shouldn't be entities
            if (ShouldSkipEntityConfiguration(clrType)) continue;

            // Skip owned entities to avoid conflicts
            if (entity.IsOwned()) continue;

            try
            {
                modelBuilder.Entity(clrType)
                            .Property<Dictionary<string, object>>("ExtraProperties")
                            .HasColumnName("ExtraProperties")
                            .HasColumnType("jsonb");
            }
            catch (System.InvalidOperationException ex) when (
                ex.Message.Contains("shared type") || 
                ex.Message.Contains("already been configured") ||
                ex.Message.Contains("cannot be configured"))
            {
                // Skip types that have configuration conflicts
                continue;
            }
        }
    }

    /// <summary>
    /// Determines if a type should be skipped for entity configuration
    /// </summary>
    private static bool ShouldSkipEntityConfiguration(Type type)
    {
        // Skip value objects and primitive types
        if (type.IsValueType || type.IsPrimitive) return true;
        
        // Skip common value object types
        var skipTypes = new[]
        {
            "Money",
            "Currency",
            "Amount",
            "Percentage",
            "Duration",
            "DateTime",
            "DateOnly",
            "TimeOnly",
            "InvoiceItem" // Skip InvoiceItem as it's configured as owned
        };

        return skipTypes.Any(skipType => type.Name.Contains(skipType, StringComparison.OrdinalIgnoreCase));
    }
} 