using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Reflection;

namespace ShipMvp.Application.Infrastructure.Data;

public static class ModelBuilderExtensions
{

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
