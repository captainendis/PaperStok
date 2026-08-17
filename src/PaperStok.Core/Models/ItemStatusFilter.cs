/*
 * Copyright (c) 2026 PaperAxis. All rights reserved.
 * This file is part of PaperStok. Unauthorized copying, modification
 * or distribution of this file is strictly prohibited.
 */
namespace PaperStok.Core.Models;

/// <summary>Which item statuses to include when reading warehouse stock.</summary>
public enum ItemStatusFilter
{
    All,
    ActiveOnly,
    PassiveOnly
}

public static class ItemStatusFilterExtensions
{
    public static IEnumerable<WarehouseStockRow> Apply(this IEnumerable<WarehouseStockRow> rows, ItemStatusFilter filter) =>
        filter switch
        {
            ItemStatusFilter.ActiveOnly => rows.Where(r => r.IsActive),
            ItemStatusFilter.PassiveOnly => rows.Where(r => !r.IsActive),
            _ => rows
        };
}
