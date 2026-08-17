/*
 * Copyright (c) 2026 PaperAxis. All rights reserved.
 * This file is part of PaperStok. Unauthorized copying, modification
 * or distribution of this file is strictly prohibited.
 */
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using Microsoft.Data.SqlClient;
using Microsoft.Win32;
using PaperStok.App.Converters;
using PaperStok.App.Reporting;
using PaperStok.Core.Export;
using PaperStok.Core.Logo;
using PaperStok.Core.Models;
using PaperStok.Core.Reporting;
using PaperStok.Core.Storage;

namespace PaperStok.App.Views;

/// <summary>
/// Pulls warehouse stock, lets the user merge warehouses into named columns
/// and pick a subset of items, then shows a pivoted item x warehouse-group
/// report with a row total column and a totals row — and can save that
/// configuration as a named, re-runnable report.
/// </summary>
public partial class StockReportWindow : Window
{
    private readonly ConnectionProfile _profile;
    private readonly LogoStockRepository _repository = new();
    private readonly ReportDefinitionStore _reportStore = new();
    private readonly List<StockReportDefinition> _definitions = [];

    private List<WarehouseStockRow> _lastPulledRows = [];
    private List<WarehouseGroupRow> _mappingRows = [];
    private List<ItemPickerRow> _allItems = [];
    private StockReportResult? _currentResult;

    public StockReportWindow(ConnectionProfile profile)
    {
        InitializeComponent();
        _profile = profile;

        _definitions.AddRange(_reportStore.Load());
        ReportCombo.ItemsSource = _definitions;

        StatusFilterCombo.ItemsSource = new List<StatusFilterItem>
        {
            new(ItemStatusFilter.All, "Aktif + Pasif"),
            new(ItemStatusFilter.ActiveOnly, "Yalnızca Aktif"),
            new(ItemStatusFilter.PassiveOnly, "Yalnızca Pasif")
        };
        StatusFilterCombo.SelectedIndex = 0;

        Title = $"PaperStok — Ambar Raporu ({profile.Name})";

        if (!string.IsNullOrWhiteSpace(profile.CustomQueryTemplate))
            StatusText.Text = "⚠ Bu profilde özel bir sorgu tanımlı — Verileri Yenile onu kullanır, varsayılan sorguyu değil.";
    }

    private ItemStatusFilter CurrentStatusFilter => (StatusFilterCombo.SelectedItem as StatusFilterItem)?.Filter ?? ItemStatusFilter.All;

    private List<WarehouseStockRow> FilteredRows => _lastPulledRows.Apply(CurrentStatusFilter).ToList();

    private async void Refresh_Click(object sender, RoutedEventArgs e) => await PullDataAsync();

    private void StatusFilterCombo_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (_lastPulledRows.Count == 0)
            return;

        RebuildMappingRows();
        RebuildItemRows();
        StatusText.Text = $"{FilteredRows.Count} satır ({CurrentStatusFilterLabel}). Ambar gruplarını düzenleyip Raporu Oluştur'a basın.";
    }

    private string CurrentStatusFilterLabel => (StatusFilterCombo.SelectedItem as StatusFilterItem)?.Label ?? "Aktif + Pasif";

    private async Task PullDataAsync()
    {
        StatusText.Text = "Veriler çekiliyor…";
        IsEnabled = false;
        try
        {
            _lastPulledRows = (await _repository.GetWarehouseTotalsAsync(_profile)).ToList();
            RebuildMappingRows();
            RebuildItemRows();
            StatusText.Text = $"{_lastPulledRows.Count} satır çekildi. Ambar gruplarını düzenleyip Raporu Oluştur'a basın.";
        }
        catch (UnsafeQueryException ex)
        {
            MessageBox.Show(this,
                $"Sorgu reddedildi — PaperStok salt okunurdur.\n\n{ex.Message}",
                "Salt Okunur Kısıtı", MessageBoxButton.OK, MessageBoxImage.Warning);
            StatusText.Text = "Veri çekme başarısız oldu.";
        }
        catch (SqlException ex)
        {
            MessageBox.Show(this,
                $"Logo Tiger3 veritabanına bağlanılamadı veya sorgu çalıştırılamadı.\n\n{ex.Message}",
                "Bağlantı Hatası", MessageBoxButton.OK, MessageBoxImage.Error);
            StatusText.Text = "Veri çekme başarısız oldu.";
        }
        catch (Exception ex)
        {
            MessageBox.Show(this, $"Beklenmeyen bir hata oluştu:\n\n{ex.Message}", "PaperStok",
                MessageBoxButton.OK, MessageBoxImage.Error);
            StatusText.Text = "Veri çekme başarısız oldu.";
        }
        finally
        {
            IsEnabled = true;
        }
    }

    /// <summary>Rebuilds the warehouse list from the last pull. Keeps each warehouse's
    /// current group-name edit if it was already known; new warehouses default to
    /// their own name (ungrouped, nothing excluded).</summary>
    private void RebuildMappingRows()
    {
        var existingByWarehouse = _mappingRows.ToDictionary(r => r.WarehouseNo, r => r.GroupName);

        _mappingRows = FilteredRows
            .Select(r => (r.WarehouseNo, r.WarehouseName))
            .Distinct()
            .OrderBy(w => w.WarehouseNo)
            .Select(w => new WarehouseGroupRow
            {
                WarehouseNo = w.WarehouseNo,
                WarehouseName = w.WarehouseName,
                GroupName = existingByWarehouse.TryGetValue(w.WarehouseNo, out var existing) ? existing : w.WarehouseName
            })
            .ToList();

        MappingGrid.ItemsSource = _mappingRows;
    }

    /// <summary>Rebuilds the item checklist from the last pull. Keeps each item's current
    /// checked state if it was already known; new items default to checked (included).</summary>
    private void RebuildItemRows()
    {
        var existingByCode = _allItems.ToDictionary(i => i.ItemCode, i => i.IsSelected, StringComparer.OrdinalIgnoreCase);

        _allItems = FilteredRows
            .Select(r => (r.ItemCode, r.ItemName))
            .Distinct()
            .OrderBy(i => i.ItemCode, StringComparer.OrdinalIgnoreCase)
            .Select(i => new ItemPickerRow
            {
                ItemCode = i.ItemCode,
                ItemName = i.ItemName,
                IsSelected = existingByCode.TryGetValue(i.ItemCode, out var selected) ? selected : true
            })
            .ToList();

        RefreshItemsListDisplay();
    }

    private void RefreshItemsListDisplay()
    {
        var query = ItemSearchBox.Text?.Trim();
        IEnumerable<ItemPickerRow> source = _allItems;
        if (!string.IsNullOrEmpty(query))
        {
            source = source.Where(i =>
                i.ItemCode.Contains(query, StringComparison.OrdinalIgnoreCase) ||
                i.ItemName.Contains(query, StringComparison.OrdinalIgnoreCase));
        }

        // Always a fresh List instance: reassigning the same reference is a
        // WPF no-op and would leave checkboxes showing stale IsSelected state.
        ItemsListBox.ItemsSource = source.ToList();
    }

    /// <summary>Forces the grid to re-read GroupName after mutating rows in place
    /// (reassigning the same list reference would be a WPF no-op — see above).</summary>
    private void RefreshMappingGridDisplay()
    {
        MappingGrid.ItemsSource = null;
        MappingGrid.ItemsSource = _mappingRows;
    }

    private void ItemSearchBox_TextChanged(object sender, TextChangedEventArgs e) => RefreshItemsListDisplay();

    private void SelectAllItems_Click(object sender, RoutedEventArgs e)
    {
        foreach (var item in _allItems)
            item.IsSelected = true;
        RefreshItemsListDisplay();
    }

    private void ClearItems_Click(object sender, RoutedEventArgs e)
    {
        foreach (var item in _allItems)
            item.IsSelected = false;
        RefreshItemsListDisplay();
    }

    private void BuildReport_Click(object sender, RoutedEventArgs e)
    {
        if (_lastPulledRows.Count == 0)
        {
            MessageBox.Show(this, "Önce \"Verileri Yenile\" ile stokları çekin.", "PaperStok",
                MessageBoxButton.OK, MessageBoxImage.Information);
            return;
        }

        var definition = BuildDefinitionFromCurrentState(ReportCombo.SelectedItem is StockReportDefinition selected ? selected.Name : "");
        _currentResult = StockReportBuilder.Build(FilteredRows, definition);
        RenderPivot(_currentResult);
        StatusText.Text = $"{_currentResult.Rows.Count} ürün, {_currentResult.GroupNames.Count} ambar sütunu.";
    }

    private StockReportDefinition BuildDefinitionFromCurrentState(string name)
    {
        var warehouseGroups = WarehouseGroup.FromMapping(_mappingRows.Select(r => (r.WarehouseNo, r.GroupName)));

        // Empty ItemCodes means "all items" — including ones added to Logo
        // later. Only save an explicit list when the user actually narrowed it.
        var allSelected = _allItems.Count > 0 && _allItems.All(i => i.IsSelected);
        List<string> itemCodes = allSelected
            ? new List<string>()
            : _allItems.Where(i => i.IsSelected).Select(i => i.ItemCode).ToList();

        return new StockReportDefinition
        {
            Name = name,
            ConnectionProfileName = _profile.Name,
            WarehouseGroups = warehouseGroups,
            ItemCodes = itemCodes
        };
    }

    private void RenderPivot(StockReportResult result)
    {
        PivotGrid.Columns.Clear();

        PivotGrid.Columns.Add(new DataGridTextColumn
        { Header = "Stok Kodu", Binding = new Binding(nameof(StockReportRow.ItemCode)), IsReadOnly = true, Width = 120 });
        PivotGrid.Columns.Add(new DataGridTextColumn
        { Header = "Stok Adı", Binding = new Binding(nameof(StockReportRow.ItemName)), IsReadOnly = true, Width = new DataGridLength(1, DataGridLengthUnitType.Star) });
        PivotGrid.Columns.Add(new DataGridTextColumn
        { Header = "Birim", Binding = new Binding(nameof(StockReportRow.Unit)), IsReadOnly = true, Width = 70 });

        var converter = new GroupQuantityConverter();
        foreach (var groupName in result.GroupNames)
        {
            PivotGrid.Columns.Add(new DataGridTextColumn
            {
                Header = groupName,
                IsReadOnly = true,
                Width = 110,
                Binding = new Binding(".") { Converter = converter, ConverterParameter = groupName, StringFormat = "{0:#,##0.###}" }
            });
        }

        PivotGrid.Columns.Add(new DataGridTextColumn
        {
            Header = "Toplam",
            IsReadOnly = true,
            Width = 110,
            Binding = new Binding(nameof(StockReportRow.Total)) { StringFormat = "{0:#,##0.###}" }
        });

        var rowsWithTotal = new List<StockReportRow>(result.Rows) { result.TotalsRow };
        PivotGrid.ItemsSource = rowsWithTotal;
    }

    private void ReportCombo_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (ReportCombo.SelectedItem is not StockReportDefinition definition)
            return;

        ApplyDefinition(definition);
    }

    private async void ApplyDefinition(StockReportDefinition definition)
    {
        if (_lastPulledRows.Count == 0)
        {
            await PullDataAsync();
            if (_lastPulledRows.Count == 0)
                return; // pull failed; PullDataAsync already showed the error
        }

        var groupNameByWarehouse = new Dictionary<int, string>();
        foreach (var group in definition.WarehouseGroups)
            foreach (var warehouseNo in group.WarehouseNumbers)
                groupNameByWarehouse[warehouseNo] = group.Name;

        foreach (var row in _mappingRows)
            row.GroupName = groupNameByWarehouse.TryGetValue(row.WarehouseNo, out var name) ? name : "";
        RefreshMappingGridDisplay();

        var itemCodeSet = definition.ItemCodes.Count > 0
            ? new HashSet<string>(definition.ItemCodes, StringComparer.OrdinalIgnoreCase)
            : null;
        foreach (var item in _allItems)
            item.IsSelected = itemCodeSet is null || itemCodeSet.Contains(item.ItemCode);
        RefreshItemsListDisplay();

        StatusText.Text = $"\"{definition.Name}\" yüklendi. Raporu Oluştur'a basın.";
    }

    private void NewReport_Click(object sender, RoutedEventArgs e)
    {
        ReportCombo.SelectedItem = null;

        foreach (var row in _mappingRows)
            row.GroupName = row.WarehouseName;
        RefreshMappingGridDisplay();

        foreach (var item in _allItems)
            item.IsSelected = true;
        RefreshItemsListDisplay();

        StatusText.Text = "Yeni rapor. Ambar gruplarını düzenleyip Raporu Oluştur'a basın.";
    }

    private void SaveReport_Click(object sender, RoutedEventArgs e)
    {
        if (ReportCombo.SelectedItem is StockReportDefinition existing)
        {
            SaveDefinition(existing.Name);
            return;
        }

        SaveReportAs_Click(sender, e);
    }

    private void SaveReportAs_Click(object sender, RoutedEventArgs e)
    {
        var dialog = new TextInputWindow("Raporu Kaydet", "Rapor adı:") { Owner = this };
        if (dialog.ShowDialog() != true)
            return;

        SaveDefinition(dialog.Value);
    }

    private void SaveDefinition(string name)
    {
        var definition = BuildDefinitionFromCurrentState(name);

        var existingIndex = _definitions.FindIndex(d => string.Equals(d.Name, name, StringComparison.OrdinalIgnoreCase));
        if (existingIndex >= 0)
            _definitions[existingIndex] = definition;
        else
            _definitions.Add(definition);

        _reportStore.Save(_definitions);
        ReportCombo.ItemsSource = null;
        ReportCombo.ItemsSource = _definitions;
        ReportCombo.SelectedItem = _definitions.FirstOrDefault(d => string.Equals(d.Name, name, StringComparison.OrdinalIgnoreCase));

        StatusText.Text = $"\"{name}\" kaydedildi.";
    }

    private void DeleteReport_Click(object sender, RoutedEventArgs e)
    {
        if (ReportCombo.SelectedItem is not StockReportDefinition selected)
        {
            MessageBox.Show(this, "Önce silinecek bir rapor seçin.", "PaperStok",
                MessageBoxButton.OK, MessageBoxImage.Information);
            return;
        }

        var confirm = MessageBox.Show(this, $"\"{selected.Name}\" raporu silinsin mi?", "PaperStok",
            MessageBoxButton.YesNo, MessageBoxImage.Warning);
        if (confirm != MessageBoxResult.Yes)
            return;

        _definitions.Remove(selected);
        _reportStore.Save(_definitions);
        ReportCombo.ItemsSource = null;
        ReportCombo.ItemsSource = _definitions;
        StatusText.Text = $"\"{selected.Name}\" silindi.";
    }

    private async void ExportExcel_Click(object sender, RoutedEventArgs e) => await ExportAsync(new ExcelStockReportExporter());

    private async void ExportCsv_Click(object sender, RoutedEventArgs e) => await ExportAsync(new CsvStockReportExporter());

    private async Task ExportAsync(IStockReportExporter exporter)
    {
        if (_currentResult is null)
        {
            MessageBox.Show(this, "Dışa aktarılacak rapor yok. Önce Raporu Oluştur'a basın.", "PaperStok",
                MessageBoxButton.OK, MessageBoxImage.Information);
            return;
        }

        var dialog = new SaveFileDialog
        {
            Filter = $"{exporter.DisplayName}|*{exporter.FileExtension}",
            FileName = $"PaperStok-Ambar-Raporu-{DateTime.Now:yyyyMMdd-HHmm}{exporter.FileExtension}"
        };
        if (dialog.ShowDialog(this) != true)
            return;

        IsEnabled = false;
        try
        {
            await exporter.ExportAsync(_currentResult, dialog.FileName);
            StatusText.Text = $"Rapor dışa aktarıldı: {dialog.FileName}";
        }
        catch (Exception ex)
        {
            MessageBox.Show(this, $"Dışa aktarma başarısız oldu:\n\n{ex.Message}", "PaperStok",
                MessageBoxButton.OK, MessageBoxImage.Error);
        }
        finally
        {
            IsEnabled = true;
        }
    }

    private sealed record StatusFilterItem(ItemStatusFilter Filter, string Label)
    {
        public override string ToString() => Label;
    }
}
