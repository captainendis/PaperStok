/*
 * Copyright (c) 2026 PaperAxis. All rights reserved.
 * This file is part of PaperStok. Unauthorized copying, modification
 * or distribution of this file is strictly prohibited.
 */
using PaperStok.Core.Models;

namespace PaperStok.Core.Logo;

/// <summary>
/// Builds the SQL used to read warehouse stock totals from a Logo Tiger3
/// Enterprise database. Table/column names follow the standard Logo
/// schema (LG_&lt;firm&gt;_&lt;period&gt;_STINVTOT, LG_&lt;firm&gt;_ITEMS,
/// L_CAPIWHOUSE). Heavily customized Logo installations can rename or add
/// columns, so every profile can override this template via
/// ConnectionProfile.CustomQueryTemplate — the placeholders below are the
/// only contract PaperStok relies on.
/// </summary>
public static class LogoQueryTemplates
{
    /// <summary>
    /// Default warehouse-totals query. {FIRM} and {PERIOD} are replaced with
    /// the profile's zero-padded firm/period suffixes before execution.
    /// </summary>
    public const string DefaultWarehouseTotalsQuery = """
        SELECT
            wh.NR              AS WarehouseNo,
            wh.NAME             AS WarehouseName,
            it.CODE             AS ItemCode,
            it.NAME             AS ItemName,
            ISNULL(un.NAME, '') AS Unit,
            SUM(iv.ONHAND)      AS OnHand,
            SUM(iv.RESERVED)    AS Reserved,
            SUM(iv.ORDERED)     AS OnOrder
        FROM LG_{FIRM}_{PERIOD}_STINVTOT iv
        INNER JOIN LG_{FIRM}_ITEMS it ON it.LOGICALREF = iv.STOCKREF
        INNER JOIN L_CAPIWHOUSE wh ON wh.NR = iv.INVENNO AND wh.FIRMNR = {FIRMNO}
        LEFT JOIN LG_{FIRM}_UNITSETF un ON un.REF = it.UNITSETREF AND un.UINFO = 0
        WHERE it.CARDTYPE = 1
        GROUP BY wh.NR, wh.NAME, it.CODE, it.NAME, un.NAME
        HAVING SUM(iv.ONHAND) <> 0 OR SUM(iv.RESERVED) <> 0
        ORDER BY wh.NR, it.CODE;
        """;

    public static string Build(ConnectionProfile profile)
    {
        var template = string.IsNullOrWhiteSpace(profile.CustomQueryTemplate)
            ? DefaultWarehouseTotalsQuery
            : profile.CustomQueryTemplate;

        return template
            .Replace("{FIRM}", profile.FirmSuffix)
            .Replace("{PERIOD}", profile.PeriodSuffix)
            .Replace("{FIRMNO}", profile.FirmNo.ToString());
    }
}
