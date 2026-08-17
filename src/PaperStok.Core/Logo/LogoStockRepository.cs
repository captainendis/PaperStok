/*
 * Copyright (c) 2026 PaperAxis. All rights reserved.
 * This file is part of PaperStok. Unauthorized copying, modification
 * or distribution of this file is strictly prohibited.
 */
using System.Runtime.Versioning;
using Microsoft.Data.SqlClient;
using PaperStok.Core.Models;

namespace PaperStok.Core.Logo;

/// <summary>Reads warehouse stock totals from a Logo Tiger3 Enterprise database.</summary>
[SupportedOSPlatform("windows")]
public sealed class LogoStockRepository
{
    public async Task TestConnectionAsync(ConnectionProfile profile, CancellationToken cancellationToken = default)
    {
        await using var connection = new SqlConnection(LogoConnectionStringBuilder.Build(profile));
        await connection.OpenAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<WarehouseStockRow>> GetWarehouseTotalsAsync(
        ConnectionProfile profile,
        CancellationToken cancellationToken = default)
    {
        var sql = LogoQueryTemplates.Build(profile);

        await using var connection = new SqlConnection(LogoConnectionStringBuilder.Build(profile));
        await connection.OpenAsync(cancellationToken);

        await using var command = new SqlCommand(sql, connection) { CommandTimeout = 120 };
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);

        var results = new List<WarehouseStockRow>();
        while (await reader.ReadAsync(cancellationToken))
        {
            results.Add(new WarehouseStockRow
            {
                WarehouseNo = GetInt32(reader, "WarehouseNo"),
                WarehouseName = GetString(reader, "WarehouseName"),
                ItemCode = GetString(reader, "ItemCode"),
                ItemName = GetString(reader, "ItemName"),
                Unit = GetString(reader, "Unit"),
                OnHand = GetDecimal(reader, "OnHand"),
                Reserved = GetDecimal(reader, "Reserved"),
                OnOrder = GetDecimal(reader, "OnOrder")
            });
        }

        return results;
    }

    // Logo's own numeric columns aren't always SQL `decimal`/`int` — e.g.
    // LG_STINVTOT.ONHAND is documented as Delphi "Double" (SQL `float`), and
    // a custom query template can return practically any numeric SQL type.
    // SqlDataReader.GetDecimal()/GetInt32() are strict — they throw
    // InvalidCastException ("Specified cast is not valid.") unless the
    // column's SQL type matches exactly. Reading the boxed value and
    // converting leniently handles int/float/real/decimal/numeric/money
    // alike, whatever the real installation's column types turn out to be.
    private static string GetString(SqlDataReader reader, string column)
    {
        var ordinal = reader.GetOrdinal(column);
        return reader.IsDBNull(ordinal) ? "" : Convert.ToString(reader.GetValue(ordinal))?.Trim() ?? "";
    }

    private static int GetInt32(SqlDataReader reader, string column)
    {
        var ordinal = reader.GetOrdinal(column);
        return reader.IsDBNull(ordinal) ? 0 : Convert.ToInt32(reader.GetValue(ordinal));
    }

    private static decimal GetDecimal(SqlDataReader reader, string column)
    {
        var ordinal = reader.GetOrdinal(column);
        return reader.IsDBNull(ordinal) ? 0m : Convert.ToDecimal(reader.GetValue(ordinal));
    }
}
