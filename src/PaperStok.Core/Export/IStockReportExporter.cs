/*
 * Copyright (c) 2026 PaperAxis. All rights reserved.
 * This file is part of PaperStok. Unauthorized copying, modification
 * or distribution of this file is strictly prohibited.
 */
using PaperStok.Core.Reporting;

namespace PaperStok.Core.Export;

public interface IStockReportExporter
{
    string DisplayName { get; }
    string FileExtension { get; }

    Task ExportAsync(StockReportResult result, string filePath, CancellationToken cancellationToken = default);
}
