/*
 * Copyright (c) 2026 PaperAxis. All rights reserved.
 * This file is part of PaperStok. Unauthorized copying, modification
 * or distribution of this file is strictly prohibited.
 */
using System.Windows;
using System.Windows.Media;
using Microsoft.Data.SqlClient;
using PaperStok.Core.Logo;
using PaperStok.Core.Models;
using PaperStok.Core.Storage;

namespace PaperStok.App.Views;

public partial class ConnectionSettingsWindow : Window
{
    private readonly ConnectionProfile? _existingProfile;
    private readonly LogoStockRepository _repository = new();

    public ConnectionProfile? Result { get; private set; }

    public ConnectionSettingsWindow(ConnectionProfile? existingProfile = null)
    {
        InitializeComponent();
        _existingProfile = existingProfile;

        if (existingProfile is not null)
        {
            Title = "Bağlantı Ayarları — Düzenle";
            NameBox.Text = existingProfile.Name;
            ServerBox.Text = existingProfile.ServerAddress;
            PortBox.Text = existingProfile.Port?.ToString() ?? "";
            DatabaseBox.Text = existingProfile.DatabaseName;
            SqlAuthRadio.IsChecked = existingProfile.AuthMode == LogoAuthMode.SqlServer;
            WindowsAuthRadio.IsChecked = existingProfile.AuthMode == LogoAuthMode.Windows;
            UsernameBox.Text = existingProfile.Username;
            FirmNoBox.Text = existingProfile.FirmNo.ToString();
            PeriodNoBox.Text = existingProfile.PeriodNo.ToString();
            EncryptCheck.IsChecked = existingProfile.EncryptConnection;
            TrustCertCheck.IsChecked = existingProfile.TrustServerCertificate;
            CustomQueryBox.Text = existingProfile.CustomQueryTemplate ?? "";
            PasswordHintText.Visibility = Visibility.Visible;
        }

        AuthMode_Changed(this, new RoutedEventArgs());
    }

    private void AuthMode_Changed(object sender, RoutedEventArgs e)
    {
        var isSqlAuth = SqlAuthRadio.IsChecked == true;
        UsernameBox.IsEnabled = isSqlAuth;
        PasswordBox.IsEnabled = isSqlAuth;
    }

    private bool TryBuildProfile(out ConnectionProfile profile, out string error)
    {
        profile = new ConnectionProfile();
        error = "";

        if (string.IsNullOrWhiteSpace(NameBox.Text))
        {
            error = "Profil adı zorunludur.";
            return false;
        }
        if (string.IsNullOrWhiteSpace(ServerBox.Text))
        {
            error = "Sunucu adresi zorunludur.";
            return false;
        }
        if (string.IsNullOrWhiteSpace(DatabaseBox.Text))
        {
            error = "Veritabanı adı zorunludur.";
            return false;
        }
        if (!int.TryParse(FirmNoBox.Text, out var firmNo) || firmNo <= 0)
        {
            error = "Firma No pozitif bir sayı olmalıdır.";
            return false;
        }
        if (!int.TryParse(PeriodNoBox.Text, out var periodNo) || periodNo <= 0)
        {
            error = "Dönem No pozitif bir sayı olmalıdır.";
            return false;
        }

        int? port = null;
        if (!string.IsNullOrWhiteSpace(PortBox.Text))
        {
            if (!int.TryParse(PortBox.Text, out var parsedPort) || parsedPort is <= 0 or > 65535)
            {
                error = "Port 1-65535 aralığında bir sayı olmalıdır.";
                return false;
            }
            port = parsedPort;
        }

        var authMode = SqlAuthRadio.IsChecked == true ? LogoAuthMode.SqlServer : LogoAuthMode.Windows;

        if (authMode == LogoAuthMode.SqlServer && string.IsNullOrWhiteSpace(UsernameBox.Text))
        {
            error = "SQL Server kimlik doğrulaması için kullanıcı adı zorunludur.";
            return false;
        }

        var typedPassword = PasswordBox.Password;
        string? protectedPassword;
        if (authMode == LogoAuthMode.Windows)
        {
            protectedPassword = null;
        }
        else if (!string.IsNullOrEmpty(typedPassword))
        {
            protectedPassword = PasswordProtector.Protect(typedPassword);
        }
        else
        {
            protectedPassword = _existingProfile?.ProtectedPassword;
        }

        profile = new ConnectionProfile
        {
            Name = NameBox.Text.Trim(),
            ServerAddress = ServerBox.Text.Trim(),
            Port = port,
            DatabaseName = DatabaseBox.Text.Trim(),
            AuthMode = authMode,
            Username = UsernameBox.Text.Trim(),
            ProtectedPassword = protectedPassword,
            FirmNo = firmNo,
            PeriodNo = periodNo,
            EncryptConnection = EncryptCheck.IsChecked == true,
            TrustServerCertificate = TrustCertCheck.IsChecked == true,
            CustomQueryTemplate = string.IsNullOrWhiteSpace(CustomQueryBox.Text) ? null : CustomQueryBox.Text
        };

        // PaperStok is read-only by design: reject any custom query here,
        // at save time, rather than letting it fail later on "Stokları Çek".
        try
        {
            LogoQueryTemplates.Build(profile);
        }
        catch (UnsafeQueryException ex)
        {
            error = $"Özel sorgu reddedildi — PaperStok salt okunurdur ve Logo Tiger3 veritabanında hiçbir değişiklik yapmaz.\n{ex.Message}";
            profile = new ConnectionProfile();
            return false;
        }

        return true;
    }

    private async void TestConnection_Click(object sender, RoutedEventArgs e)
    {
        if (!TryBuildProfile(out var profile, out var error))
        {
            ShowStatus(error, isError: true);
            return;
        }

        ShowStatus("Bağlantı test ediliyor…", isError: false);
        IsEnabled = false;
        try
        {
            await _repository.TestConnectionAsync(profile);
            ShowStatus("Bağlantı başarılı.", isError: false);
        }
        catch (SqlException ex)
        {
            ShowStatus($"Bağlantı başarısız: {ex.Message}", isError: true);
        }
        catch (Exception ex)
        {
            ShowStatus($"Beklenmeyen hata: {ex.Message}", isError: true);
        }
        finally
        {
            IsEnabled = true;
        }
    }

    private void ShowStatus(string message, bool isError)
    {
        StatusText.Text = message;
        StatusText.Foreground = isError
            ? (Brush)FindResource("Pa.Danger")
            : (Brush)FindResource("Pa.Success");
        StatusText.Visibility = Visibility.Visible;
    }

    private void Save_Click(object sender, RoutedEventArgs e)
    {
        if (!TryBuildProfile(out var profile, out var error))
        {
            ShowStatus(error, isError: true);
            return;
        }

        Result = profile;
        DialogResult = true;
        Close();
    }

    private void Cancel_Click(object sender, RoutedEventArgs e)
    {
        DialogResult = false;
        Close();
    }
}
