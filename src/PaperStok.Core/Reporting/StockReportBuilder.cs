/*
 * Copyright (c) 2026 PaperAxis. All rights reserved.
 * This file is part of PaperStok. Unauthorized copying, modification
 * or distribution of this file is strictly prohibited.
 */
using PaperStok.Core.Models;

namespace PaperStok.Core.Reporting;

/// <summary>
/// Pivots flat per-warehouse stock rows into a per-item x per-warehouse-group
/// report: warehouses can be merged into a shared column (or left out of the
/// report entirely), items can be filtered to a chosen subset, and both a
/// row total (last column) and a totals row (last row) are computed.
/// </summary>
public static class StockReportBuilder
{
    public static StockReportResult Build(IReadOnlyList<WarehouseStockRow> sourceRows, StockReportDefinition definition)
    {
        var groups = definition.WarehouseGroups.Count > 0
            ? definition.WarehouseGroups
            : DefaultGroups(sourceRows);

        var warehouseToGroup = new Dictionary<int, string>();
        foreach (var group in groups)
        {
            foreach (var warehouseNo in group.WarehouseNumbers)
                warehouseToGroup[warehouseNo] = group.Name;
        }

        var itemFilter = definition.ItemCodes.Count > 0
            ? new HashSet<string>(definition.ItemCodes, StringComparer.OrdinalIgnoreCase)
            : null;

        var rowsByItem = new Dictionary<string, StockReportRow>(StringComparer.OrdinalIgnoreCase);
        foreach (var source in sourceRows)
        {
            if (!warehouseToGroup.TryGetValue(source.WarehouseNo, out var groupName))
                continue;

            if (itemFilter is not null && !itemFilter.Contains(source.ItemCode))
                continue;

            if (!rowsByItem.TryGetValue(source.ItemCode, out var row))
            {
                row = new StockReportRow { ItemCode = source.ItemCode, ItemName = source.ItemName, Unit = source.Unit };
                rowsByItem[source.ItemCode] = row;
            }

            row.Quantities[groupName] = row.QuantityFor(groupName) + source.OnHand;
        }

        var rows = rowsByItem.Values
            .OrderBy(r => r.ItemCode, StringComparer.OrdinalIgnoreCase)
            .ToList();
        foreach (var row in rows)
            row.Total = row.Quantities.Values.Sum();

        var groupNames = groups.Select(g => g.Name).Distinct().ToList();

        var totalsRow = new StockReportRow { ItemName = "TOPLAM" };
        foreach (var groupName in groupNames)
            totalsRow.Quantities[groupName] = rows.Sum(r => r.QuantityFor(groupName));
        totalsRow.Total = totalsRow.Quantities.Values.Sum();

        return new StockReportResult { GroupNames = groupNames, Rows = rows, TotalsRow = totalsRow };
    }

    /// <summary>No saved groups yet: one column per warehouse, named after it, nothing excluded.</summary>
    private static List<WarehouseGroup> DefaultGroups(IReadOnlyList<WarehouseStockRow> sourceRows) =>
        sourceRows
            .Select(r => (r.WarehouseNo, r.WarehouseName))
            .Distinct()
            .OrderBy(w => w.WarehouseNo)
            .Select(w => new WarehouseGroup { Name = w.WarehouseName, WarehouseNumbers = [w.WarehouseNo] })
            .ToList();
}
