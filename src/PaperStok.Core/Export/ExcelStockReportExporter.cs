/*
 * Copyright (c) 2026 PaperAxis. All rights reserved.
 * This file is part of PaperStok. Unauthorized copying, modification
 * or distribution of this file is strictly prohibited.
 */
using ClosedXML.Excel;
using PaperStok.Core.Reporting;

namespace PaperStok.Core.Export;

/// <summary>Writes a pivoted stock report as an Excel sheet, totals row and column included.</summary>
public sealed class ExcelStockReportExporter : IStockReportExporter
{
    public string DisplayName => "Excel (.xlsx)";
    public string FileExtension => ".xlsx";

    public Task ExportAsync(StockReportResult result, string filePath, CancellationToken cancellationToken = default)
    {
        using var workbook = new XLWorkbook();
        var sheet = workbook.Worksheets.Add("Ambar Raporu");

        var headers = new List<string> { "Stok Kodu", "Stok Adı", "Birim" };
        headers.AddRange(result.GroupNames);
        headers.Add("Toplam");

        for (var col = 0; col < headers.Count; col++)
            sheet.Cell(1, col + 1).Value = headers[col];

        var headerRange = sheet.Range(1, 1, 1, headers.Count);
        headerRange.Style.Font.Bold = true;
        headerRange.Style.Fill.BackgroundColor = XLColor.FromHtml("#0A1B30");
        headerRange.Style.Font.FontColor = XLColor.White;

        var rowIndex = 2;
        foreach (var row in result.Rows)
        {
            cancellationToken.ThrowIfCancellationRequested();
            WriteRow(sheet, rowIndex, row, result.GroupNames);
            rowIndex++;
        }

        var totalsRowIndex = rowIndex;
        WriteRow(sheet, totalsRowIndex, result.TotalsRow, result.GroupNames);
        var totalsRange = sheet.Range(totalsRowIndex, 1, totalsRowIndex, headers.Count);
        totalsRange.Style.Font.Bold = true;
        totalsRange.Style.Fill.BackgroundColor = XLColor.FromHtml("#EFF4FA");

        var quantityRange = sheet.Range(2, 4, totalsRowIndex, headers.Count);
        quantityRange.Style.NumberFormat.Format = "#,##0.###";

        sheet.SheetView.FreezeRows(1);
        sheet.Range(1, 1, totalsRowIndex, headers.Count).SetAutoFilter();
        sheet.Columns().AdjustToContents();

        workbook.SaveAs(filePath);
        return Task.CompletedTask;
    }

    private static void WriteRow(IXLWorksheet sheet, int rowIndex, StockReportRow row, IReadOnlyList<string> groupNames)
    {
        sheet.Cell(rowIndex, 1).Value = row.ItemCode;
        sheet.Cell(rowIndex, 2).Value = row.ItemName;
        sheet.Cell(rowIndex, 3).Value = row.Unit;

        var col = 4;
        foreach (var groupName in groupNames)
        {
            sheet.Cell(rowIndex, col).Value = row.QuantityFor(groupName);
            col++;
        }

        sheet.Cell(rowIndex, col).Value = row.Total;
    }
}
