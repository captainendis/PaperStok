/*
 * Copyright (c) 2026 PaperAxis. All rights reserved.
 * This file is part of PaperStok. Unauthorized copying, modification
 * or distribution of this file is strictly prohibited.
 */
namespace PaperStok.Core.Reporting;

/// <summary>One item's on-hand quantity per report warehouse column, plus its row total.</summary>
public sealed class StockReportRow
{
    public string ItemCode { get; set; } = "";
    public string ItemName { get; set; } = "";
    public string Unit { get; set; } = "";

    /// <summary>On-hand quantity per warehouse group name (the pivoted columns).</summary>
    public Dictionary<string, decimal> Quantities { get; set; } = new();

    /// <summary>Sum of <see cref="Quantities"/> across every group — the report's last column.</summary>
    public decimal Total { get; set; }

    public decimal QuantityFor(string groupName) => Quantities.GetValueOrDefault(groupName);
}
