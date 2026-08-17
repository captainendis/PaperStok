/*
 * Copyright (c) 2026 PaperAxis. All rights reserved.
 * This file is part of PaperStok. Unauthorized copying, modification
 * or distribution of this file is strictly prohibited.
 */
using PaperStok.Core.Models;
using Xunit;

namespace PaperStok.Core.Tests;

public class WarehouseGroupTests
{
    [Fact]
    public void FromMapping_MergesWarehousesWithTheSameGroupName()
    {
        var mapping = new List<(int, string)>
        {
            (1, "Merkez"),
            (3, "Merkez"),
            (2, "Beylikdüzü")
        };

        var groups = WarehouseGroup.FromMapping(mapping);

        Assert.Equal(2, groups.Count);
        var merkez = Assert.Single(groups, g => g.Name == "Merkez");
        Assert.Equal([1, 3], merkez.WarehouseNumbers);
        var beylikduzu = Assert.Single(groups, g => g.Name == "Beylikdüzü");
        Assert.Equal([2], beylikduzu.WarehouseNumbers);
    }

    [Fact]
    public void FromMapping_ExcludesBlankGroupNames()
    {
        var mapping = new List<(int, string)>
        {
            (1, "Merkez"),
            (9, "   ")
        };

        var groups = WarehouseGroup.FromMapping(mapping);

        var group = Assert.Single(groups);
        Assert.Equal("Merkez", group.Name);
    }

    [Fact]
    public void FromMapping_TrimsAndIgnoresCaseWhenMerging()
    {
        var mapping = new List<(int, string)>
        {
            (1, "Merkez"),
            (2, " merkez ")
        };

        var groups = WarehouseGroup.FromMapping(mapping);

        var group = Assert.Single(groups);
        Assert.Equal([1, 2], group.WarehouseNumbers);
    }
}
