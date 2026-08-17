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
                WarehouseNo = reader.GetInt32(reader.GetOrdinal("WarehouseNo")),
                WarehouseName = reader.GetString(reader.GetOrdinal("WarehouseName")).Trim(),
                ItemCode = reader.GetString(reader.GetOrdinal("ItemCode")).Trim(),
                ItemName = reader.GetString(reader.GetOrdinal("ItemName")).Trim(),
                Unit = reader.GetString(reader.GetOrdinal("Unit")).Trim(),
                OnHand = reader.GetDecimal(reader.GetOrdinal("OnHand")),
                Reserved = reader.GetDecimal(reader.GetOrdinal("Reserved")),
                OnOrder = reader.GetDecimal(reader.GetOrdinal("OnOrder"))
            });
        }

        return results;
    }
}
