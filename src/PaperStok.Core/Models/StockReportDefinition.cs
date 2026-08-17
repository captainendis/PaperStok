/*
 * Copyright (c) 2026 PaperAxis. All rights reserved.
 * This file is part of PaperStok. Unauthorized copying, modification
 * or distribution of this file is strictly prohibited.
 */
namespace PaperStok.Core.Models;

/// <summary>
/// A saved, re-runnable warehouse report: which warehouses to show (and how
/// to merge them into named columns) and which items to include. Saved to
/// disk so the same report can be pulled again later without rebuilding it.
/// </summary>
public sealed class StockReportDefinition
{
    public string Name { get; set; } = "";

    /// <summary>Which connection profile this was last built against (informational only).</summary>
    public string ConnectionProfileName { get; set; } = "";

    /// <summary>
    /// Report columns. A warehouse not listed in any group is left out of
    /// the report entirely. Empty means "one column per warehouse" (no
    /// merging, nothing excluded) — the default, ungrouped view.
    /// </summary>
    public List<WarehouseGroup> WarehouseGroups { get; set; } = [];

    /// <summary>Stock item codes to include. Empty means "all items".</summary>
    public List<string> ItemCodes { get; set; } = [];
}
