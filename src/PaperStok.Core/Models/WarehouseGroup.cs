/*
 * Copyright (c) 2026 PaperAxis. All rights reserved.
 * This file is part of PaperStok. Unauthorized copying, modification
 * or distribution of this file is strictly prohibited.
 */
namespace PaperStok.Core.Models;

/// <summary>
/// A named report column that combines one or more real Logo warehouses.
/// E.g. Name="Merkez", WarehouseNumbers=[1, 3] merges "Merkez Depo" (1) and
/// "Geçici Depo" (3) into a single "Merkez" column in a stock report.
/// </summary>
public sealed class WarehouseGroup
{
    public string Name { get; set; } = "";
    public List<int> WarehouseNumbers { get; set; } = [];

    /// <summary>
    /// Builds groups from a flat "one row per real warehouse" editable mapping
    /// (warehouse number, chosen group name). Warehouses sharing the same
    /// (trimmed, case-insensitive) group name are merged into one group;
    /// a blank/whitespace group name excludes that warehouse from the report.
    /// </summary>
    public static List<WarehouseGroup> FromMapping(IEnumerable<(int WarehouseNo, string GroupName)> mapping) =>
        mapping
            .Where(m => !string.IsNullOrWhiteSpace(m.GroupName))
            .GroupBy(m => m.GroupName.Trim(), StringComparer.OrdinalIgnoreCase)
            .Select(g => new WarehouseGroup { Name = g.Key, WarehouseNumbers = g.Select(m => m.WarehouseNo).ToList() })
            .ToList();
}
