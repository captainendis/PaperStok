/*
 * Copyright (c) 2026 PaperAxis. All rights reserved.
 * This file is part of PaperStok. Unauthorized copying, modification
 * or distribution of this file is strictly prohibited.
 */
using System.Runtime.Versioning;
using Microsoft.Data.SqlClient;
using PaperStok.Core.Models;
using PaperStok.Core.Storage;

namespace PaperStok.Core.Logo;

[SupportedOSPlatform("windows")]
public static class LogoConnectionStringBuilder
{
    public static string Build(ConnectionProfile profile)
    {
        var dataSource = profile.Port is { } port
            ? $"{profile.ServerAddress},{port}"
            : profile.ServerAddress;

        var builder = new SqlConnectionStringBuilder
        {
            DataSource = dataSource,
            InitialCatalog = profile.DatabaseName,
            Encrypt = profile.EncryptConnection,
            TrustServerCertificate = profile.TrustServerCertificate,
            ConnectTimeout = 10,
            // PaperStok never writes to Logo Tiger3. This hints the server to
            // route to a readable secondary when the database is part of an
            // Always On availability group; it is a no-op otherwise. The
            // authoritative read-only guarantee is the SQL login's own
            // permissions (grant it db_datareader only) plus SqlReadOnlyGuard.
            ApplicationIntent = ApplicationIntent.ReadOnly
        };

        if (profile.AuthMode == LogoAuthMode.Windows)
        {
            builder.IntegratedSecurity = true;
        }
        else
        {
            builder.UserID = profile.Username;
            builder.Password = PasswordProtector.Unprotect(profile.ProtectedPassword) ?? "";
        }

        return builder.ConnectionString;
    }
}
