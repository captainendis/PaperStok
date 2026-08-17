/*
 * Copyright (c) 2026 PaperAxis. All rights reserved.
 * This file is part of PaperStok. Unauthorized copying, modification
 * or distribution of this file is strictly prohibited.
 */
using System.Text.Json;
using PaperStok.Core.Models;

namespace PaperStok.Core.Storage;

/// <summary>
/// Persists saved warehouse-report definitions as a JSON file next to the
/// executable, the same portable pattern as <see cref="ConnectionProfileStore"/>.
/// </summary>
public sealed class ReportDefinitionStore
{
    private static readonly JsonSerializerOptions JsonOptions = new() { WriteIndented = true };

    private readonly string _filePath;

    public ReportDefinitionStore(string? filePath = null)
    {
        _filePath = filePath ?? Path.Combine(AppContext.BaseDirectory, "reports.json");
    }

    public List<StockReportDefinition> Load()
    {
        if (!File.Exists(_filePath))
            return [];

        var json = File.ReadAllText(_filePath);
        if (string.IsNullOrWhiteSpace(json))
            return [];

        return JsonSerializer.Deserialize<List<StockReportDefinition>>(json, JsonOptions) ?? [];
    }

    public void Save(IEnumerable<StockReportDefinition> definitions)
    {
        var json = JsonSerializer.Serialize(definitions, JsonOptions);
        File.WriteAllText(_filePath, json);
    }
}
