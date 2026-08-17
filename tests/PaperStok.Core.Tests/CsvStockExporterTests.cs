/*
 * Copyright (c) 2026 PaperAxis. All rights reserved.
 * This file is part of PaperStok. Unauthorized copying, modification
 * or distribution of this file is strictly prohibited.
 */
using PaperStok.Core.Export;
using PaperStok.Core.Models;
using Xunit;

namespace PaperStok.Core.Tests;

public class CsvStockExporterTests
{
    [Fact]
    public async Task ExportAsync_WritesHeaderAndRows()
    {
        var rows = new List<WarehouseStockRow>
        {
            new()
            {
                WarehouseNo = 1, WarehouseName = "Merkez Ambar", ItemCode = "STK-001",
                ItemName = "Test Ürün; Özel", Unit = "AD", OnHand = 100.5m, Reserved = 20m, OnOrder = 5m
            }
        };

        var path = Path.Combine(Path.GetTempPath(), $"paperstok-test-{Guid.NewGuid():N}.csv");
        try
        {
            await new CsvStockExporter().ExportAsync(rows, path);
            var lines = await File.ReadAllLinesAsync(path);

            Assert.Equal("Ambar Adı;Stok Kodu;Stok Adı;Birim;Eldeki;Rezerve;Sipariş;Kullanılabilir", lines[0]);
            Assert.Equal("Merkez Ambar;STK-001;\"Test Ürün; Özel\";AD;100,5;20;5;80,5", lines[1]);
        }
        finally
        {
            File.Delete(path);
        }
    }
}
