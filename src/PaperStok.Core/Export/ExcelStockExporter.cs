/*
 * Copyright (c) 2026 PaperAxis. All rights reserved.
 * This file is part of PaperStok. Unauthorized copying, modification
 * or distribution of this file is strictly prohibited.
 */
using ClosedXML.Excel;
using PaperStok.Core.Models;

namespace PaperStok.Core.Export;

public sealed class ExcelStockExporter : IStockExporter
{
    public string DisplayName => "Excel (.xlsx)";
    public string FileExtension => ".xlsx";

    private static readonly string[] Headers =
    [
        "Ambar No", "Ambar Adı", "Stok Kodu", "Stok Adı", "Birim",
        "Eldeki", "Rezerve", "Sipariş", "Kullanılabilir"
    ];

    public Task ExportAsync(IReadOnlyList<WarehouseStockRow> rows, string filePath, CancellationToken cancellationToken = default)
    {
        using var workbook = new XLWorkbook();
        var sheet = workbook.Worksheets.Add("Ambar Stok Toplamları");

        for (var col = 0; col < Headers.Length; col++)
            sheet.Cell(1, col + 1).Value = Headers[col];

        var headerRange = sheet.Range(1, 1, 1, Headers.Length);
        headerRange.Style.Font.Bold = true;
        headerRange.Style.Fill.BackgroundColor = XLColor.FromHtml("#0A1B30");
        headerRange.Style.Font.FontColor = XLColor.White;

        var rowIndex = 2;
        foreach (var row in rows)
        {
            cancellationToken.ThrowIfCancellationRequested();

            sheet.Cell(rowIndex, 1).Value = row.WarehouseNo;
            sheet.Cell(rowIndex, 2).Value = row.WarehouseName;
            sheet.Cell(rowIndex, 3).Value = row.ItemCode;
            sheet.Cell(rowIndex, 4).Value = row.ItemName;
            sheet.Cell(rowIndex, 5).Value = row.Unit;
            sheet.Cell(rowIndex, 6).Value = row.OnHand;
            sheet.Cell(rowIndex, 7).Value = row.Reserved;
            sheet.Cell(rowIndex, 8).Value = row.OnOrder;
            sheet.Cell(rowIndex, 9).Value = row.Available;
            rowIndex++;
        }

        var quantityRange = sheet.Range(2, 6, Math.Max(rowIndex - 1, 2), 9);
        quantityRange.Style.NumberFormat.Format = "#,##0.###";

        sheet.SheetView.FreezeRows(1);
        sheet.RangeUsed()?.SetAutoFilter();
        sheet.Columns().AdjustToContents();

        workbook.SaveAs(filePath);
        return Task.CompletedTask;
    }
}
