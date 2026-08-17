/*
 * Copyright (c) 2026 PaperAxis. All rights reserved.
 * This file is part of PaperStok. Unauthorized copying, modification
 * or distribution of this file is strictly prohibited.
 */
using System.Globalization;
using System.Text;
using PaperStok.Core.Reporting;

namespace PaperStok.Core.Export;

/// <summary>Writes a pivoted stock report as semicolon-delimited CSV, totals row included.</summary>
public sealed class CsvStockReportExporter : IStockReportExporter
{
    public string DisplayName => "CSV (;)";
    public string FileExtension => ".csv";

    private static readonly CultureInfo Culture = new("tr-TR");

    public async Task ExportAsync(StockReportResult result, string filePath, CancellationToken cancellationToken = default)
    {
        await using var writer = new StreamWriter(filePath, false, new UTF8Encoding(true));

        var headerCells = new List<string> { "Stok Kodu", "Stok Adı", "Birim" };
        headerCells.AddRange(result.GroupNames);
        headerCells.Add("Toplam");
        await writer.WriteLineAsync(string.Join(';', headerCells));

        foreach (var row in result.Rows)
        {
            cancellationToken.ThrowIfCancellationRequested();
            await writer.WriteLineAsync(string.Join(';', BuildCells(row, result.GroupNames)));
        }

        await writer.WriteLineAsync(string.Join(';', BuildCells(result.TotalsRow, result.GroupNames)));
    }

    private static IEnumerable<string> BuildCells(StockReportRow row, IReadOnlyList<string> groupNames)
    {
        yield return Escape(row.ItemCode);
        yield return Escape(row.ItemName);
        yield return Escape(row.Unit);
        foreach (var groupName in groupNames)
            yield return row.QuantityFor(groupName).ToString(Culture);
        yield return row.Total.ToString(Culture);
    }

    private static string Escape(string value)
    {
        if (value.Contains(';') || value.Contains('"') || value.Contains('\n'))
            return $"\"{value.Replace("\"", "\"\"")}\"";
        return value;
    }
}
