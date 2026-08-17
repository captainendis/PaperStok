/*
 * Copyright (c) 2026 PaperAxis. All rights reserved.
 * This file is part of PaperStok. Unauthorized copying, modification
 * or distribution of this file is strictly prohibited.
 */
using PaperStok.Core.Models;
using PaperStok.Core.Storage;
using Xunit;

namespace PaperStok.Core.Tests;

public class ReportDefinitionStoreTests
{
    [Fact]
    public void Load_ReturnsEmptyList_WhenFileMissing()
    {
        var path = Path.Combine(Path.GetTempPath(), $"paperstok-test-{Guid.NewGuid():N}.json");

        var definitions = new ReportDefinitionStore(path).Load();

        Assert.Empty(definitions);
    }

    [Fact]
    public void SaveAndLoad_RoundTripsGroupsAndItemFilter()
    {
        var path = Path.Combine(Path.GetTempPath(), $"paperstok-test-{Guid.NewGuid():N}.json");
        try
        {
            var store = new ReportDefinitionStore(path);
            var definition = new StockReportDefinition
            {
                Name = "Merkez + Beylikdüzü",
                ConnectionProfileName = "Üretim",
                WarehouseGroups =
                [
                    new WarehouseGroup { Name = "Merkez", WarehouseNumbers = [1, 3] },
                    new WarehouseGroup { Name = "Beylikdüzü", WarehouseNumbers = [2] }
                ],
                ItemCodes = ["STK-1", "STK-2"]
            };

            store.Save([definition]);
            var loaded = store.Load();

            var reloaded = Assert.Single(loaded);
            Assert.Equal("Merkez + Beylikdüzü", reloaded.Name);
            Assert.Equal(2, reloaded.WarehouseGroups.Count);
            Assert.Equal([1, 3], reloaded.WarehouseGroups[0].WarehouseNumbers);
            Assert.Equal(["STK-1", "STK-2"], reloaded.ItemCodes);
        }
        finally
        {
            if (File.Exists(path))
                File.Delete(path);
        }
    }
}
