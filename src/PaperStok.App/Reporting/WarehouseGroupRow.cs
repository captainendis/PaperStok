/*
 * Copyright (c) 2026 PaperAxis. All rights reserved.
 * This file is part of PaperStok. Unauthorized copying, modification
 * or distribution of this file is strictly prohibited.
 */
namespace PaperStok.App.Reporting;

/// <summary>
/// One editable row in the warehouse-mapping grid: a real Logo warehouse and
/// the report column name it's assigned to. Warehouses sharing the same
/// GroupName are merged; a blank GroupName excludes the warehouse from the
/// report. See PaperStok.Core.Models.WarehouseGroup.FromMapping.
/// </summary>
public sealed class WarehouseGroupRow
{
    public int WarehouseNo { get; set; }
    public string WarehouseName { get; set; } = "";
    public string GroupName { get; set; } = "";
}
