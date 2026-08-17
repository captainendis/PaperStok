/*
 * Copyright (c) 2026 PaperAxis. All rights reserved.
 * This file is part of PaperStok. Unauthorized copying, modification
 * or distribution of this file is strictly prohibited.
 */
using System.Reflection;

namespace PaperStok.Core;

/// <summary>
/// PaperStok künye bilgileri. Sürüm assembly metadata'sından okunur;
/// başka bir dosyada tekrar tanımlanmaz (bkz. Directory.Build.props).
/// </summary>
public static class AppInfo
{
    public const string ProductName = "PaperStok";
    public const string CompanyName = "PaperAxis";
    public const string ContactEmail = "info@paperaxis.com";
    public const string Website = "https://paperaxis.com";
    public const string PrivacyPolicyUrl = "https://paperaxis.com/gizlilik";
    public const string CopyrightYear = "2026";

    public static string Version
    {
        get
        {
            var version = Assembly.GetExecutingAssembly().GetName().Version;
            return version is null ? "0.1.0" : $"{version.Major}.{version.Minor}.{version.Build}";
        }
    }

    public static string VersionWithPrefix => $"v{Version}";

    public static string CopyrightLine => $"© {CopyrightYear} {CompanyName}. Tüm hakları saklıdır.";

    public static string AboutText =>
        $"""
        {ProductName}
        Sürüm {Version}

        Bir {CompanyName} ürünüdür.
        {CopyrightLine}

        İletişim: {ContactEmail}
        """;

    public static string FooterText => $"© {CopyrightYear} {CompanyName} · {ProductName} {VersionWithPrefix}";
}
