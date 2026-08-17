/*
 * Copyright (c) 2026 PaperAxis. All rights reserved.
 * This file is part of PaperStok. Unauthorized copying, modification
 * or distribution of this file is strictly prohibited.
 */
using PaperStok.Core.Models;

namespace PaperStok.Core.Export;

public interface IStockExporter
{
    string DisplayName { get; }
    string FileExtension { get; }

    Task ExportAsync(IReadOnlyList<WarehouseStockRow> rows, string filePath, CancellationToken cancellationToken = default);
}
