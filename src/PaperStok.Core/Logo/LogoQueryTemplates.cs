/*
 * Copyright (c) 2026 PaperAxis. All rights reserved.
 * This file is part of PaperStok. Unauthorized copying, modification
 * or distribution of this file is strictly prohibited.
 */
using PaperStok.Core.Models;

namespace PaperStok.Core.Logo;

/// <summary>
/// Builds the SQL used to read warehouse stock totals from a Logo Tiger3
/// Enterprise database.
///
/// Every table and column this query touches — L_CAPIWHOUSE (NR, NAME,
/// FIRMNR), LG_&lt;firm&gt;_&lt;period&gt;_STINVTOT (STOCKREF, INVENNO, ONHAND,
/// RESERVED, ACTPORDER), LG_&lt;firm&gt;_ITEMS (CODE, NAME, CARDTYPE, ACTIVE,
/// UNITSETREF) and LG_&lt;firm&gt;_UNITSETL (UNITSETREF, MAINUNIT, CODE) — was
/// confirmed field-by-field against a live customer database via
/// INFORMATION_SCHEMA.COLUMNS, run as a read-only discovery query through
/// PaperStok itself. That live check also caught two things two independent
/// written sources (logoisortagim.com.tr and github.com/ugurozpinar/Logo)
/// got wrong for this installation: the warehouse table is L_CAPIWHOUSE,
/// not L_CAPIDEF (which doesn't exist there — "Invalid object name"), and
/// LG_UNITSETF has no UINFO column (the unit code lives on UNITSETL instead).
///
/// Heavily customized Logo installations can still differ, so every profile
/// can override this template via ConnectionProfile.CustomQueryTemplate —
/// the placeholders below are the only contract PaperStok relies on.
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
            ISNULL(un.CODE, '')  AS Unit,
            SUM(iv.ONHAND)      AS OnHand,
            SUM(iv.RESERVED)    AS Reserved,
            SUM(iv.ACTPORDER)   AS OnOrder
        FROM LG_{FIRM}_{PERIOD}_STINVTOT iv
        INNER JOIN LG_{FIRM}_ITEMS it ON it.LOGICALREF = iv.STOCKREF
        INNER JOIN L_CAPIWHOUSE wh ON wh.NR = iv.INVENNO AND wh.FIRMNR = {FIRMNO}
        LEFT JOIN LG_{FIRM}_UNITSETL un ON un.UNITSETREF = it.UNITSETREF AND un.MAINUNIT = 1
        WHERE it.CARDTYPE = 1 AND it.ACTIVE = 0 AND iv.INVENNO <> -1
        GROUP BY wh.NR, wh.NAME, it.CODE, it.NAME, un.CODE
        HAVING SUM(iv.ONHAND) <> 0 OR SUM(iv.RESERVED) <> 0
        ORDER BY wh.NR, it.CODE;
        """;

    /// <summary>
    /// Builds the final, ready-to-execute SQL for a profile. Always passes
    /// the result through <see cref="SqlReadOnlyGuard"/> — PaperStok must
    /// never issue a query that could write to the Logo Tiger3 database,
    /// whether the query came from this default template or a profile's
    /// custom one.
    /// </summary>
    public static string Build(ConnectionProfile profile)
    {
        var template = string.IsNullOrWhiteSpace(profile.CustomQueryTemplate)
            ? DefaultWarehouseTotalsQuery
            : profile.CustomQueryTemplate;

        var sql = template
            .Replace("{FIRM}", profile.FirmSuffix)
            .Replace("{PERIOD}", profile.PeriodSuffix)
            .Replace("{FIRMNO}", profile.FirmNo.ToString());

        SqlReadOnlyGuard.EnsureReadOnly(sql);
        return sql;
    }
}
