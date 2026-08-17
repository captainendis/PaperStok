/*
 * Copyright (c) 2026 PaperAxis. All rights reserved.
 * This file is part of PaperStok. Unauthorized copying, modification
 * or distribution of this file is strictly prohibited.
 */
using ClosedXML.Excel;
using PaperStok.Core.Export;
using PaperStok.Core.Models;
using Xunit;

namespace PaperStok.Core.Tests;

public class ExcelStockExporterTests
{
    [Fact]
    public async Task ExportAsync_WritesHeaderRowAndValues()
    {
        var rows = new List<WarehouseStockRow>
        {
            new() { WarehouseNo = 2, WarehouseName = "Şube Ambarı", ItemCode = "STK-002", ItemName = "Kalem", Unit = "AD", OnHand = 10m, Reserved = 3m, OnOrder = 0m }
        };

        var path = Path.Combine(Path.GetTempPath(), $"paperstok-test-{Guid.NewGuid():N}.xlsx");
        try
        {
            await new ExcelStockExporter().ExportAsync(rows, path);

            using var workbook = new XLWorkbook(path);
            var sheet = workbook.Worksheet(1);

            Assert.Equal("Ambar No", sheet.Cell(1, 1).GetString());
            Assert.Equal("Kullanılabilir", sheet.Cell(1, 9).GetString());
            Assert.Equal(2, sheet.Cell(2, 1).GetValue<int>());
            Assert.Equal("Şube Ambarı", sheet.Cell(2, 2).GetString());
            Assert.Equal(7m, sheet.Cell(2, 9).GetValue<decimal>());
        }
        finally
        {
            File.Delete(path);
        }
    }
}
