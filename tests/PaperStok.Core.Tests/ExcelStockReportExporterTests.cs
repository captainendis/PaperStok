/*
 * Copyright (c) 2026 PaperAxis. All rights reserved.
 * This file is part of PaperStok. Unauthorized copying, modification
 * or distribution of this file is strictly prohibited.
 */
using ClosedXML.Excel;
using PaperStok.Core.Export;
using PaperStok.Core.Reporting;
using Xunit;

namespace PaperStok.Core.Tests;

public class ExcelStockReportExporterTests
{
    [Fact]
    public async Task ExportAsync_WritesHeaderRowsAndTotalsRow()
    {
        var result = new StockReportResult
        {
            GroupNames = ["Merkez", "Beylikdüzü"],
            Rows =
            [
                new StockReportRow
                {
                    ItemCode = "STK-1", ItemName = "Ürün 1", Unit = "AD",
                    Quantities = new Dictionary<string, decimal> { ["Merkez"] = 10m, ["Beylikdüzü"] = 5m },
                    Total = 15m
                }
            ],
            TotalsRow = new StockReportRow
            {
                ItemName = "TOPLAM",
                Quantities = new Dictionary<string, decimal> { ["Merkez"] = 10m, ["Beylikdüzü"] = 5m },
                Total = 15m
            }
        };

        var path = Path.Combine(Path.GetTempPath(), $"paperstok-test-{Guid.NewGuid():N}.xlsx");
        try
        {
            await new ExcelStockReportExporter().ExportAsync(result, path);

            using var workbook = new XLWorkbook(path);
            var sheet = workbook.Worksheet(1);

            Assert.Equal("Stok Kodu", sheet.Cell(1, 1).GetString());
            Assert.Equal("Merkez", sheet.Cell(1, 4).GetString());
            Assert.Equal("Toplam", sheet.Cell(1, 6).GetString());

            Assert.Equal("STK-1", sheet.Cell(2, 1).GetString());
            Assert.Equal(10m, sheet.Cell(2, 4).GetValue<decimal>());
            Assert.Equal(15m, sheet.Cell(2, 6).GetValue<decimal>());

            Assert.Equal("TOPLAM", sheet.Cell(3, 2).GetString());
            Assert.Equal(5m, sheet.Cell(3, 5).GetValue<decimal>());
        }
        finally
        {
            File.Delete(path);
        }
    }
}
