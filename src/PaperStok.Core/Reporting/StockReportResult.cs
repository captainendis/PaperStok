/*
 * Copyright (c) 2026 PaperAxis. All rights reserved.
 * This file is part of PaperStok. Unauthorized copying, modification
 * or distribution of this file is strictly prohibited.
 */
namespace PaperStok.Core.Reporting;

/// <summary>
/// A pivoted warehouse stock report: one row per item, one column per
/// warehouse group, a row total per item and a totals row per column.
/// </summary>
public sealed class StockReportResult
{
    /// <summary>Column names in display order (warehouse group names, post-merge).</summary>
    public List<string> GroupNames { get; set; } = [];

    public List<StockReportRow> Rows { get; set; } = [];

    /// <summary>Column totals across every row, plus the grand total.</summary>
    public StockReportRow TotalsRow { get; set; } = new() { ItemName = "TOPLAM" };
}
