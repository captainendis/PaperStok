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
using PaperStok.App;
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
    private readonly List<ConnectionProfile> _profiles = [];
    private List<ProfileCheckRow> _profileRows = [];
    private readonly ObservableCollection<WarehouseStockRow> _rows = [];
    private readonly ICollectionView _rowsView;

    public MainWindow()
    {
        InitializeComponent();

        _profiles.AddRange(_profileStore.Load());
        RebuildProfileRows();

        _rowsView = CollectionViewSource.GetDefaultView(_rows);
        _rowsView.Filter = FilterRow;
        StockGrid.ItemsSource = _rowsView;
        DataGridColumnTools.AttachVisibilityMenu(StockGrid);

        WarehouseFilterCombo.ItemsSource = new List<WarehouseFilterItem> { WarehouseFilterItem.All };
        WarehouseFilterCombo.SelectedIndex = 0;

        StatusFilterCombo.ItemsSource = new List<StatusFilterItem>
        {
            new(ItemStatusFilter.All, "Aktif + Pasif"),
            new(ItemStatusFilter.ActiveOnly, "Yalnızca Aktif"),
            new(ItemStatusFilter.PassiveOnly, "Yalnızca Pasif")
        };
        StatusFilterCombo.SelectedIndex = 0;

        FooterText.Text = AppInfo.FooterText;
        UpdateFirmPeriodText();
    }

    /// <summary>The row highlighted in the list — target of Düzenle/Sil, not necessarily checked for pulling.</summary>
    private ConnectionProfile? CurrentProfile => (ProfilesListBox.SelectedItem as ProfileCheckRow)?.Profile;

    /// <summary>Every profile the user has checked — these are pulled together into one table.</summary>
    private List<ConnectionProfile> CheckedProfiles => _profileRows.Where(r => r.IsChecked).Select(r => r.Profile).ToList();

    /// <summary>Rebuilds the checklist from _profiles, preserving each surviving profile's checked
    /// state by name. On first load (nothing has ever been checked), checks just the first profile
    /// so existing single-installation users see the same behavior as before.</summary>
    private void RebuildProfileRows()
    {
        var wasChecked = _profileRows.ToDictionary(r => r.Profile.Name, r => r.IsChecked, StringComparer.OrdinalIgnoreCase);
        var isFirstBuild = _profileRows.Count == 0 && wasChecked.Count == 0;

        _profileRows = _profiles
            .Select(p => new ProfileCheckRow { Profile = p, IsChecked = wasChecked.TryGetValue(p.Name, out var was) && was })
            .ToList();

        if (isFirstBuild && _profileRows.Count > 0)
            _profileRows[0].IsChecked = true;

        ProfilesListBox.ItemsSource = null;
        ProfilesListBox.ItemsSource = _profileRows;
        UpdateFirmPeriodText();
    }

    private void UpdateFirmPeriodText()
    {
        var checkedProfiles = CheckedProfiles;
        FirmPeriodText.Text = checkedProfiles.Count switch
        {
            0 => "—",
            1 => $"{checkedProfiles[0].Name} ({checkedProfiles[0].FirmSuffix} / {checkedProfiles[0].PeriodSuffix})",
            _ => $"{checkedProfiles.Count} profil: {string.Join(", ", checkedProfiles.Select(p => p.Name))}"
        };

        CustomQueryWarningText.Visibility = checkedProfiles.Any(p => !string.IsNullOrWhiteSpace(p.CustomQueryTemplate))
            ? Visibility.Visible
            : Visibility.Collapsed;
    }

    private void ProfileCheck_Changed(object sender, RoutedEventArgs e) => UpdateFirmPeriodText();

    private void SaveProfiles() => _profileStore.Save(_profiles);

    private void NewProfile_Click(object sender, RoutedEventArgs e)
    {
        var dialog = new ConnectionSettingsWindow { Owner = this };
        if (dialog.ShowDialog() != true || dialog.Result is null)
            return;

        _profiles.Add(dialog.Result);
        SaveProfiles();
        RebuildProfileRows();
        ProfilesListBox.SelectedItem = _profileRows.FirstOrDefault(r => r.Profile == dialog.Result);
    }

    private void EditProfile_Click(object sender, RoutedEventArgs e)
    {
        if (CurrentProfile is not { } current)
        {
            MessageBox.Show(this, "Önce düzenlenecek bir profil seçin.", "PaperStok", MessageBoxButton.OK, MessageBoxImage.Information);
            return;
        }

        var dialog = new ConnectionSettingsWindow(current) { Owner = this };
        if (dialog.ShowDialog() != true || dialog.Result is null)
            return;

        var index = _profiles.IndexOf(current);
        _profiles[index] = dialog.Result;
        SaveProfiles();
        RebuildProfileRows();
        ProfilesListBox.SelectedItem = _profileRows.FirstOrDefault(r => r.Profile == dialog.Result);
    }

    private void DeleteProfile_Click(object sender, RoutedEventArgs e)
    {
        if (CurrentProfile is not { } current)
        {
            MessageBox.Show(this, "Önce silinecek bir profil seçin.", "PaperStok", MessageBoxButton.OK, MessageBoxImage.Information);
            return;
        }

        var confirm = MessageBox.Show(this, $"\"{current.Name}\" profili silinsin mi?", "PaperStok",
            MessageBoxButton.YesNo, MessageBoxImage.Warning);
        if (confirm != MessageBoxResult.Yes)
            return;

        _profiles.Remove(current);
        SaveProfiles();
        RebuildProfileRows();
    }

    private async void PullStock_Click(object sender, RoutedEventArgs e)
    {
        var profiles = CheckedProfiles;
        if (profiles.Count == 0)
        {
            MessageBox.Show(this, "Önce en az bir bağlantı profili işaretleyin.", "PaperStok", MessageBoxButton.OK, MessageBoxImage.Information);
            return;
        }

        SetBusy(true, "Stoklar çekiliyor…");

        var allRows = new List<WarehouseStockRow>();
        foreach (var profile in profiles)
        {
            try
            {
                allRows.AddRange(await _repository.GetWarehouseTotalsAsync(profile));
            }
            catch (UnsafeQueryException ex)
            {
                MessageBox.Show(this,
                    $"\"{profile.Name}\" profili reddedildi — PaperStok salt okunurdur ve Logo Tiger3 veritabanında hiçbir değişiklik yapmaz.\n\n{ex.Message}\n\n" +
                    "Bağlantı Ayarları içindeki özel SQL şablonunu düzeltin.",
                    "Salt Okunur Kısıtı", MessageBoxButton.OK, MessageBoxImage.Warning);
                SetBusy(false, "Stok çekme başarısız oldu.");
                return;
            }
            catch (SqlException ex)
            {
                MessageBox.Show(this,
                    $"\"{profile.Name}\" profiliyle Logo Tiger3 veritabanına bağlanılamadı veya sorgu çalıştırılamadı.\n\n{ex.Message}\n\n" +
                    "Firma/Dönem numaralarını ve gerekiyorsa Bağlantı Ayarları içindeki özel SQL şablonunu kontrol edin.",
                    "Bağlantı Hatası", MessageBoxButton.OK, MessageBoxImage.Error);
                SetBusy(false, "Stok çekme başarısız oldu.");
                return;
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, $"\"{profile.Name}\" profili için beklenmeyen bir hata oluştu:\n\n{ex.Message}", "PaperStok",
                    MessageBoxButton.OK, MessageBoxImage.Error);
                SetBusy(false, "Stok çekme başarısız oldu.");
                return;
            }
        }

        _rows.Clear();
        foreach (var row in allRows)
            _rows.Add(row);

        RebuildWarehouseFilter();
        var profileNames = string.Join(", ", profiles.Select(p => p.Name));
        StatusText.Text = $"{allRows.Count} kayıt çekildi — {profileNames}.";
        SetBusy(false, StatusText.Text);
    }

    private void RebuildWarehouseFilter()
    {
        var distinctSources = _rows.Select(r => r.SourceProfileName).Distinct().Count();

        var warehouses = _rows
            .Select(r => (r.SourceProfileName, r.WarehouseNo, r.WarehouseName))
            .Distinct()
            .OrderBy(w => w.SourceProfileName).ThenBy(w => w.WarehouseNo)
            .Select(w => new WarehouseFilterItem(
                w.SourceProfileName,
                w.WarehouseNo,
                distinctSources > 1 ? $"{w.SourceProfileName} — {w.WarehouseName}" : w.WarehouseName))
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

        if (WarehouseFilterCombo.SelectedItem is WarehouseFilterItem { WarehouseNo: { } warehouseNo } warehouseFilter &&
            (row.WarehouseNo != warehouseNo || row.SourceProfileName != warehouseFilter.SourceProfileName))
        {
            return false;
        }

        if (StatusFilterCombo.SelectedItem is StatusFilterItem statusFilter)
        {
            if (statusFilter.Filter == ItemStatusFilter.ActiveOnly && !row.IsActive)
                return false;
            if (statusFilter.Filter == ItemStatusFilter.PassiveOnly && row.IsActive)
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

    private void AutoFitColumns_Click(object sender, RoutedEventArgs e) => DataGridColumnTools.AutoFitColumns(StockGrid);

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
        if (CurrentProfile is not { } profile)
        {
            MessageBox.Show(this, "Önce listeden bir bağlantı profili seçin (Ambar Raporu tek profille çalışır).", "PaperStok",
                MessageBoxButton.OK, MessageBoxImage.Information);
            return;
        }

        new StockReportWindow(profile) { Owner = this }.Show();
    }

    private void About_Click(object sender, RoutedEventArgs e) => new AboutWindow { Owner = this }.ShowDialog();

    private void Exit_Click(object sender, RoutedEventArgs e) => Close();

    /// <summary>One checkable connection profile in the sidebar list.</summary>
    private sealed class ProfileCheckRow
    {
        public required ConnectionProfile Profile { get; init; }
        public string Name => Profile.Name;
        public bool IsChecked { get; set; }
    }

    private sealed record WarehouseFilterItem(string? SourceProfileName, int? WarehouseNo, string Label)
    {
        public static readonly WarehouseFilterItem All = new(null, null, "Tüm Ambarlar");
        public override string ToString() => Label;
    }

    private sealed record StatusFilterItem(ItemStatusFilter Filter, string Label)
    {
        public override string ToString() => Label;
    }
}
