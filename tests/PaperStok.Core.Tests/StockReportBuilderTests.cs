/*
 * Copyright (c) 2026 PaperAxis. All rights reserved.
 * This file is part of PaperStok. Unauthorized copying, modification
 * or distribution of this file is strictly prohibited.
 */
using PaperStok.Core.Models;
using PaperStok.Core.Reporting;
using Xunit;

namespace PaperStok.Core.Tests;

public class StockReportBuilderTests
{
    private static WarehouseStockRow Row(int whNo, string whName, string itemCode, string itemName, decimal onHand) => new()
    {
        WarehouseNo = whNo,
        WarehouseName = whName,
        ItemCode = itemCode,
        ItemName = itemName,
        Unit = "AD",
        OnHand = onHand
    };

    [Fact]
    public void Build_WithNoGroups_CreatesOneColumnPerWarehouse()
    {
        var source = new List<WarehouseStockRow>
        {
            Row(1, "Merkez Depo", "STK-1", "Ürün 1", 10m),
            Row(2, "Beylikdüzü Depo", "STK-1", "Ürün 1", 5m)
        };

        var result = StockReportBuilder.Build(source, new StockReportDefinition());

        Assert.Equal(["Merkez Depo", "Beylikdüzü Depo"], result.GroupNames);
        var row = Assert.Single(result.Rows);
        Assert.Equal(10m, row.QuantityFor("Merkez Depo"));
        Assert.Equal(5m, row.QuantityFor("Beylikdüzü Depo"));
        Assert.Equal(15m, row.Total);
    }

    [Fact]
    public void Build_MergesWarehousesAssignedToTheSameGroup()
    {
        // Merkez Depo (1) ve Geçici Depo (3) "Merkez" grubunda birleşiyor.
        var source = new List<WarehouseStockRow>
        {
            Row(1, "Merkez Depo", "STK-1", "Ürün 1", 10m),
            Row(3, "Geçici Depo", "STK-1", "Ürün 1", 4m),
            Row(2, "Beylikdüzü Depo", "STK-1", "Ürün 1", 5m)
        };
        var definition = new StockReportDefinition
        {
            WarehouseGroups =
            [
                new WarehouseGroup { Name = "Merkez", WarehouseNumbers = [1, 3] },
                new WarehouseGroup { Name = "Beylikdüzü", WarehouseNumbers = [2] }
            ]
        };

        var result = StockReportBuilder.Build(source, definition);

        Assert.Equal(["Merkez", "Beylikdüzü"], result.GroupNames);
        var row = Assert.Single(result.Rows);
        Assert.Equal(14m, row.QuantityFor("Merkez"));
        Assert.Equal(5m, row.QuantityFor("Beylikdüzü"));
        Assert.Equal(19m, row.Total);
    }

    [Fact]
    public void Build_ExcludesWarehousesNotAssignedToAnyGroup()
    {
        var source = new List<WarehouseStockRow>
        {
            Row(1, "Merkez Depo", "STK-1", "Ürün 1", 10m),
            Row(9, "Kullanılmayan Depo", "STK-1", "Ürün 1", 99m)
        };
        var definition = new StockReportDefinition
        {
            WarehouseGroups = [new WarehouseGroup { Name = "Merkez", WarehouseNumbers = [1] }]
        };

        var result = StockReportBuilder.Build(source, definition);

        var row = Assert.Single(result.Rows);
        Assert.Equal(10m, row.Total);
        Assert.DoesNotContain("Kullanılmayan Depo", result.GroupNames);
    }

    [Fact]
    public void Build_FiltersToSelectedItemCodesOnly()
    {
        var source = new List<WarehouseStockRow>
        {
            Row(1, "Merkez Depo", "STK-1", "Ürün 1", 10m),
            Row(1, "Merkez Depo", "STK-2", "Ürün 2", 20m)
        };
        var definition = new StockReportDefinition { ItemCodes = ["STK-2"] };

        var result = StockReportBuilder.Build(source, definition);

        var row = Assert.Single(result.Rows);
        Assert.Equal("STK-2", row.ItemCode);
    }

    [Fact]
    public void Build_ComputesTotalsRowAcrossAllItems()
    {
        var source = new List<WarehouseStockRow>
        {
            Row(1, "Merkez Depo", "STK-1", "Ürün 1", 10m),
            Row(2, "Beylikdüzü Depo", "STK-1", "Ürün 1", 5m),
            Row(1, "Merkez Depo", "STK-2", "Ürün 2", 3m),
            Row(2, "Beylikdüzü Depo", "STK-2", "Ürün 2", 7m)
        };

        var result = StockReportBuilder.Build(source, new StockReportDefinition());

        Assert.Equal(13m, result.TotalsRow.QuantityFor("Merkez Depo"));
        Assert.Equal(12m, result.TotalsRow.QuantityFor("Beylikdüzü Depo"));
        Assert.Equal(25m, result.TotalsRow.Total);
    }

    [Fact]
    public void Build_RowsAreSortedByItemCode()
    {
        var source = new List<WarehouseStockRow>
        {
            Row(1, "Merkez Depo", "STK-9", "Z Ürün", 1m),
            Row(1, "Merkez Depo", "STK-1", "A Ürün", 1m)
        };

        var result = StockReportBuilder.Build(source, new StockReportDefinition());

        Assert.Equal(["STK-1", "STK-9"], result.Rows.Select(r => r.ItemCode));
    }
}
