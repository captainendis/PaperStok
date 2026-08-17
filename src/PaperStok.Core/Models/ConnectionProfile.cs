/*
 * Copyright (c) 2026 PaperAxis. All rights reserved.
 * This file is part of PaperStok. Unauthorized copying, modification
 * or distribution of this file is strictly prohibited.
 */
namespace PaperStok.Core.Models;

public enum LogoAuthMode
{
    SqlServer,
    Windows
}

/// <summary>
/// Which STINVTOT object the default query reads stock totals from. Some
/// Logo Tiger3 installations keep LG_&lt;firm&gt;_&lt;period&gt;_STINVTOT (the table)
/// up to date via a totals-recalculation job; others don't run that job, in
/// which case only LV_&lt;firm&gt;_&lt;period&gt;_STINVTOT (the equivalent live view)
/// reflects current stock. View is the default because it always reflects
/// live data regardless of whether that job runs.
/// </summary>
public enum StockSourceKind
{
    View,
    Table
}

/// <summary>
/// Logo Tiger3 Enterprise MSSQL bağlantısı için kaydedilen bir profil.
/// Parola diskte her zaman şifreli tutulur (bkz. ConnectionProfileStore).
/// </summary>
public sealed class ConnectionProfile
{
    public string Name { get; set; } = "";
    public string ServerAddress { get; set; } = "";
    public int? Port { get; set; }
    public string DatabaseName { get; set; } = "";
    public LogoAuthMode AuthMode { get; set; } = LogoAuthMode.SqlServer;
    public string Username { get; set; } = "";

    /// <summary>DPAPI ile şifrelenmiş parola (Base64). Bkz. ConnectionProfileStore.</summary>
    public string? ProtectedPassword { get; set; }

    /// <summary>Logo firma numarası (LG_&lt;firmNo&gt;_... tablo ön eki).</summary>
    public int FirmNo { get; set; } = 1;

    /// <summary>Logo dönem numarası (LG_&lt;firmNo&gt;_&lt;periodNo&gt;_... tablo ön eki).</summary>
    public int PeriodNo { get; set; } = 1;

    public bool EncryptConnection { get; set; } = true;
    public bool TrustServerCertificate { get; set; } = true;

    /// <summary>
    /// Ambar toplamları sorgusu için özelleştirilmiş SQL şablonu.
    /// Boş bırakılırsa LogoQueryTemplates.DefaultWarehouseTotalsQuery kullanılır.
    /// </summary>
    public string? CustomQueryTemplate { get; set; }

    /// <summary>Varsayılan sorgunun stok toplamlarını LG_ tablosundan mı LV_ görünümünden mü okuyacağı.</summary>
    public StockSourceKind StockSource { get; set; } = StockSourceKind.View;

    public string FirmSuffix => FirmNo.ToString("000");
    public string PeriodSuffix => PeriodNo.ToString("00");
}
