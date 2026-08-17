/*
 * Copyright (c) 2026 PaperAxis. All rights reserved.
 * This file is part of PaperStok. Unauthorized copying, modification
 * or distribution of this file is strictly prohibited.
 */
using System.Globalization;
using System.Text;
using PaperStok.Core.Models;

namespace PaperStok.Core.Export;

/// <summary>
/// Writes semicolon-delimited CSV (Turkish regional Excel default) with a
/// UTF-8 BOM so Türkçe karakterler Excel'de bozulmadan açılır.
/// </summary>
public sealed class CsvStockExporter : IStockExporter
{
    public string DisplayName => "CSV (;)";
    public string FileExtension => ".csv";

    private static readonly CultureInfo Culture = new("tr-TR");

    public async Task ExportAsync(IReadOnlyList<WarehouseStockRow> rows, string filePath, CancellationToken cancellationToken = default)
    {
        await using var writer = new StreamWriter(filePath, false, new UTF8Encoding(true));

        await writer.WriteLineAsync(string.Join(';',
            "Ambar No", "Ambar Adı", "Stok Kodu", "Stok Adı", "Birim",
            "Eldeki", "Rezerve", "Sipariş", "Kullanılabilir"));

        foreach (var row in rows)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var line = string.Join(';',
                row.WarehouseNo.ToString(Culture),
                Escape(row.WarehouseName),
                Escape(row.ItemCode),
                Escape(row.ItemName),
                Escape(row.Unit),
                row.OnHand.ToString(Culture),
                row.Reserved.ToString(Culture),
                row.OnOrder.ToString(Culture),
                row.Available.ToString(Culture));

            await writer.WriteLineAsync(line);
        }
    }

    private static string Escape(string value)
    {
        if (value.Contains(';') || value.Contains('"') || value.Contains('\n'))
            return $"\"{value.Replace("\"", "\"\"")}\"";
        return value;
    }
}
