using Microsoft.SemanticKernel;
using System;
using System.Linq;

namespace ShipMvp.Integration.SemanticKernel.Plugins;

/// <summary>Detects if a column is historical or forecast.</summary>
public sealed class DetectYearTypeSkill
{
    [KernelFunction]
    public string[] Detect(string[] colHeaders)
    {
        // Heuristic: If header is <= current year, treat as historical; else forecast.
        var now = DateTime.UtcNow.Year;
        return colHeaders.Select(h =>
        {
            if (int.TryParse(h, out var y) && y <= now) return "historical";
            return "forecast";
        }).ToArray();
    }
}
