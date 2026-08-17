/*
 * Copyright (c) 2026 PaperAxis. All rights reserved.
 * This file is part of PaperStok. Unauthorized copying, modification
 * or distribution of this file is strictly prohibited.
 */
using PaperStok.Core.Export;
using PaperStok.Core.Reporting;
using Xunit;

namespace PaperStok.Core.Tests;

public class CsvStockReportExporterTests
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

        var path = Path.Combine(Path.GetTempPath(), $"paperstok-test-{Guid.NewGuid():N}.csv");
        try
        {
            await new CsvStockReportExporter().ExportAsync(result, path);
            var lines = await File.ReadAllLinesAsync(path);

            Assert.Equal("Stok Kodu;Stok Adı;Birim;Merkez;Beylikdüzü;Toplam", lines[0]);
            Assert.Equal("STK-1;Ürün 1;AD;10;5;15", lines[1]);
            Assert.Equal(";TOPLAM;;10;5;15", lines[2]);
        }
        finally
        {
            File.Delete(path);
        }
    }
}
