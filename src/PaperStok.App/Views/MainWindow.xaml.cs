/*
 * Copyright (c) 2026 PaperAxis. All rights reserved.
 * This file is part of PaperStok. Unauthorized copying, modification
 * or distribution of this file is strictly prohibited.
 */
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;
using System.Windows.Data;
using Microsoft.Data.SqlClient;
using Microsoft.Win32;
using PaperStok.Core;
using PaperStok.Core.Export;
using PaperStok.Core.Logo;
using PaperStok.Core.Models;
using PaperStok.Core.Storage;

namespace PaperStok.App.Views;

public partial class MainWindow : Window
{
    private readonly ConnectionProfileStore _profileStore = new();
    private readonly LogoStockRepository _repository = new();
    private readonly ObservableCollection<ConnectionProfile> _profiles = [];
    private readonly ObservableCollection<WarehouseStockRow> _rows = [];
    private readonly ICollectionView _rowsView;

    public MainWindow()
    {
        InitializeComponent();

        foreach (var profile in _profileStore.Load())
            _profiles.Add(profile);

        ProfileCombo.ItemsSource = _profiles;
        if (_profiles.Count > 0)
            ProfileCombo.SelectedIndex = 0;

        _rowsView = CollectionViewSource.GetDefaultView(_rows);
        _rowsView.Filter = FilterRow;
        StockGrid.ItemsSource = _rowsView;

        WarehouseFilterCombo.ItemsSource = new List<WarehouseFilterItem> { WarehouseFilterItem.All };
        WarehouseFilterCombo.SelectedIndex = 0;

        FooterText.Text = AppInfo.FooterText;
        UpdateFirmPeriodText();
    }

    private ConnectionProfile? SelectedProfile => ProfileCombo.SelectedItem as ConnectionProfile;

    private void UpdateFirmPeriodText()
    {
        FirmPeriodText.Text = SelectedProfile is { } p ? $"{p.FirmSuffix} / {p.PeriodSuffix}" : "—";
    }

    private void ProfileCombo_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        => UpdateFirmPeriodText();

    private void SaveProfiles() => _profileStore.Save(_profiles);

    private void NewProfile_Click(object sender, RoutedEventArgs e)
    {
        var dialog = new ConnectionSettingsWindow { Owner = this };
        if (dialog.ShowDialog() != true || dialog.Result is null)
            return;

        _profiles.Add(dialog.Result);
        ProfileCombo.SelectedItem = dialog.Result;
        SaveProfiles();
    }

    private void EditProfile_Click(object sender, RoutedEventArgs e)
    {
        if (SelectedProfile is not { } current)
        {
            MessageBox.Show(this, "Önce bir bağlantı profili seçin.", "PaperStok", MessageBoxButton.OK, MessageBoxImage.Information);
            return;
        }

        var dialog = new ConnectionSettingsWindow(current) { Owner = this };
        if (dialog.ShowDialog() != true || dialog.Result is null)
            return;

        var index = _profiles.IndexOf(current);
        _profiles[index] = dialog.Result;
        ProfileCombo.SelectedItem = dialog.Result;
        SaveProfiles();
    }

    private void DeleteProfile_Click(object sender, RoutedEventArgs e)
    {
        if (SelectedProfile is not { } current)
        {
            MessageBox.Show(this, "Önce bir bağlantı profili seçin.", "PaperStok", MessageBoxButton.OK, MessageBoxImage.Information);
            return;
        }

        var confirm = MessageBox.Show(this, $"\"{current.Name}\" profili silinsin mi?", "PaperStok",
            MessageBoxButton.YesNo, MessageBoxImage.Warning);
        if (confirm != MessageBoxResult.Yes)
            return;

        _profiles.Remove(current);
        SaveProfiles();
        UpdateFirmPeriodText();
    }

    private async void PullStock_Click(object sender, RoutedEventArgs e)
    {
        if (SelectedProfile is not { } profile)
        {
            MessageBox.Show(this, "Önce bir bağlantı profili seçin.", "PaperStok", MessageBoxButton.OK, MessageBoxImage.Information);
            return;
        }

        SetBusy(true, "Stoklar çekiliyor…");
        try
        {
            var result = await _repository.GetWarehouseTotalsAsync(profile);

            _rows.Clear();
            foreach (var row in result)
                _rows.Add(row);

            RebuildWarehouseFilter();
            StatusText.Text = $"{result.Count} kayıt çekildi — {SelectedProfile?.Name}.";
        }
        catch (UnsafeQueryException ex)
        {
            MessageBox.Show(this,
                $"Sorgu reddedildi — PaperStok salt okunurdur ve Logo Tiger3 veritabanında hiçbir değişiklik yapmaz.\n\n{ex.Message}\n\n" +
                "Bağlantı Ayarları içindeki özel SQL şablonunu düzeltin.",
                "Salt Okunur Kısıtı", MessageBoxButton.OK, MessageBoxImage.Warning);
            StatusText.Text = "Stok çekme başarısız oldu.";
        }
        catch (SqlException ex)
        {
            MessageBox.Show(this,
                $"Logo Tiger3 veritabanına bağlanılamadı veya sorgu çalıştırılamadı.\n\n{ex.Message}\n\n" +
                "Firma/Dönem numaralarını ve gerekiyorsa Bağlantı Ayarları içindeki özel SQL şablonunu kontrol edin.",
                "Bağlantı Hatası", MessageBoxButton.OK, MessageBoxImage.Error);
            StatusText.Text = "Stok çekme başarısız oldu.";
        }
        catch (Exception ex)
        {
            MessageBox.Show(this, $"Beklenmeyen bir hata oluştu:\n\n{ex.Message}", "PaperStok",
                MessageBoxButton.OK, MessageBoxImage.Error);
            StatusText.Text = "Stok çekme başarısız oldu.";
        }
        finally
        {
            SetBusy(false, StatusText.Text);
        }
    }

    private void RebuildWarehouseFilter()
    {
        var warehouses = _rows
            .Select(r => (r.WarehouseNo, r.WarehouseName))
            .Distinct()
            .OrderBy(w => w.WarehouseNo)
            .Select(w => new WarehouseFilterItem(w.WarehouseNo, $"{w.WarehouseNo} — {w.WarehouseName}"))
            .ToList();

        warehouses.Insert(0, WarehouseFilterItem.All);
        WarehouseFilterCombo.ItemsSource = warehouses;
        WarehouseFilterCombo.SelectedIndex = 0;
    }

    private void SetBusy(bool isBusy, string? statusMessage = null)
    {
        BusyBar.Visibility = isBusy ? Visibility.Visible : Visibility.Collapsed;
        PullStockButton.IsEnabled = !isBusy;
        if (statusMessage is not null)
            StatusText.Text = statusMessage;
    }

    private bool FilterRow(object obj)
    {
        if (obj is not WarehouseStockRow row)
            return false;

        if (WarehouseFilterCombo.SelectedItem is WarehouseFilterItem { WarehouseNo: { } warehouseNo } &&
            row.WarehouseNo != warehouseNo)
        {
            return false;
        }

        var query = SearchBox.Text?.Trim();
        if (string.IsNullOrEmpty(query))
            return true;

        return row.ItemCode.Contains(query, StringComparison.OrdinalIgnoreCase)
            || row.ItemName.Contains(query, StringComparison.OrdinalIgnoreCase);
    }

    private void Filter_Changed(object sender, System.Windows.Controls.TextChangedEventArgs e) => _rowsView.Refresh();

    private void Filter_Changed(object sender, System.Windows.Controls.SelectionChangedEventArgs e) => _rowsView.Refresh();

    private async void ExportExcel_Click(object sender, RoutedEventArgs e) => await ExportAsync(new ExcelStockExporter());

    private async void ExportCsv_Click(object sender, RoutedEventArgs e) => await ExportAsync(new CsvStockExporter());

    private async Task ExportAsync(IStockExporter exporter)
    {
        var visibleRows = _rowsView.Cast<WarehouseStockRow>().ToList();
        if (visibleRows.Count == 0)
        {
            MessageBox.Show(this, "Dışa aktarılacak kayıt yok. Önce stokları çekin.", "PaperStok",
                MessageBoxButton.OK, MessageBoxImage.Information);
            return;
        }

        var dialog = new SaveFileDialog
        {
            Filter = $"{exporter.DisplayName}|*{exporter.FileExtension}",
            FileName = $"PaperStok-Ambar-Stok-{DateTime.Now:yyyyMMdd-HHmm}{exporter.FileExtension}"
        };
        if (dialog.ShowDialog(this) != true)
            return;

        SetBusy(true, "Dışa aktarılıyor…");
        try
        {
            await exporter.ExportAsync(visibleRows, dialog.FileName);
            StatusText.Text = $"{visibleRows.Count} kayıt dışa aktarıldı: {dialog.FileName}";
        }
        catch (Exception ex)
        {
            MessageBox.Show(this, $"Dışa aktarma başarısız oldu:\n\n{ex.Message}", "PaperStok",
                MessageBoxButton.OK, MessageBoxImage.Error);
        }
        finally
        {
            SetBusy(false, StatusText.Text);
        }
    }

    private void StockReport_Click(object sender, RoutedEventArgs e)
    {
        if (SelectedProfile is not { } profile)
        {
            MessageBox.Show(this, "Önce bir bağlantı profili seçin.", "PaperStok", MessageBoxButton.OK, MessageBoxImage.Information);
            return;
        }

        new StockReportWindow(profile) { Owner = this }.Show();
    }

    private void About_Click(object sender, RoutedEventArgs e) => new AboutWindow { Owner = this }.ShowDialog();

    private void Exit_Click(object sender, RoutedEventArgs e) => Close();

    private sealed record WarehouseFilterItem(int? WarehouseNo, string Label)
    {
        public static readonly WarehouseFilterItem All = new(null, "Tüm Ambarlar");
        public override string ToString() => Label;
    }
}
