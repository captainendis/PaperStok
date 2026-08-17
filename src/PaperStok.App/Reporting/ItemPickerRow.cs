/*
 * Copyright (c) 2026 PaperAxis. All rights reserved.
 * This file is part of PaperStok. Unauthorized copying, modification
 * or distribution of this file is strictly prohibited.
 */
namespace PaperStok.App.Reporting;

/// <summary>One selectable stock item in the report's item picker checklist.</summary>
public sealed class ItemPickerRow
{
    public string ItemCode { get; set; } = "";
    public string ItemName { get; set; } = "";
    public bool IsSelected { get; set; }

    public string Display => $"{ItemCode} — {ItemName}";
}
