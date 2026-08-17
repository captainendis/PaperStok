/*
 * Copyright (c) 2026 PaperAxis. All rights reserved.
 * This file is part of PaperStok. Unauthorized copying, modification
 * or distribution of this file is strictly prohibited.
 */
using PaperStok.Core.Models;
using Xunit;

namespace PaperStok.Core.Tests;

public class ItemStatusFilterTests
{
    private static readonly List<WarehouseStockRow> Rows =
    [
        new() { ItemCode = "A", IsActive = true },
        new() { ItemCode = "B", IsActive = false }
    ];

    [Fact]
    public void Apply_All_ReturnsEverything()
    {
        var result = Rows.Apply(ItemStatusFilter.All).ToList();
        Assert.Equal(2, result.Count);
    }

    [Fact]
    public void Apply_ActiveOnly_ExcludesPassive()
    {
        var result = Rows.Apply(ItemStatusFilter.ActiveOnly).ToList();
        Assert.Single(result);
        Assert.Equal("A", result[0].ItemCode);
    }

    [Fact]
    public void Apply_PassiveOnly_ExcludesActive()
    {
        var result = Rows.Apply(ItemStatusFilter.PassiveOnly).ToList();
        Assert.Single(result);
        Assert.Equal("B", result[0].ItemCode);
    }
}
