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
/// The query itself doesn't filter by item card type or active/passive
/// status — it returns everything and reports ITEMS.ACTIVE per row as
/// IsActive, so PaperStok's UI can let the user choose active-only,
/// passive-only or both without a round trip to the database.
///
/// Every table and column this query touches — L_CAPIWHOUSE (NR, NAME,
/// FIRMNR), LV_&lt;firm&gt;_&lt;period&gt;_STINVTOT (STOCKREF, INVENNO, ONHAND,
/// RESERVED, ACTPORDER), LG_&lt;firm&gt;_ITEMS (CODE, NAME, ACTIVE,
/// UNITSETREF) and LG_&lt;firm&gt;_UNITSETL (UNITSETREF, MAINUNIT, CODE) — was
/// confirmed field-by-field against a live customer database via
/// INFORMATION_SCHEMA.COLUMNS, run as a read-only discovery query through
/// PaperStok itself. That live check also caught things two independent
/// written sources (logoisortagim.com.tr and github.com/ugurozpinar/Logo)
/// got wrong for this installation: the warehouse table is L_CAPIWHOUSE,
/// not L_CAPIDEF (which doesn't exist there — "Invalid object name"), and
/// LG_UNITSETF has no UINFO column (the unit code lives on UNITSETL instead).
///
/// STINVTOT defaults to the LV_ prefix (the view), not LG_ (the table): on
/// the installation this was first verified against, LG_&lt;firm&gt;_&lt;period&gt;_STINVTOT
/// exists but is never populated (the "totals recalculation" job that would
/// maintain it isn't run), while LV_&lt;firm&gt;_&lt;period&gt;_STINVTOT is a live
/// view with the identical column set that computes current on-hand/reserved
/// quantities on the fly. Installations where that job does run can switch a
/// profile to the table via ConnectionProfile.StockSource — Bağlantı
/// Ayarları exposes this as "Tablo (LG_)" / "Görünüm (LV_)". Other tables
/// (ITEMS, UNITSETL) are plain master-data tables, not periodically
/// recalculated aggregates, so LG_ is fine for those regardless.
///
/// A profile can also point at a Logo database reachable only through a SQL
/// Server linked server rather than the database PaperStok connects to
/// directly: set ConnectionProfile.LinkedServerName (and LinkedServerDatabase)
/// and every table reference below gets a four-part-name prefix
/// (LinkedServerName.LinkedServerDatabase.dbo.) instead of resolving locally.
/// This still runs as an ordinary qualified SELECT — not OPENQUERY/OPENROWSET,
/// which SqlReadOnlyGuard blocks outright — so it stays within what the guard
/// can actually verify.
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
            SUM(iv.ACTPORDER)   AS OnOrder,
            CASE WHEN it.ACTIVE = 0 THEN 1 ELSE 0 END AS IsActive
        FROM {DB_PREFIX}{STINVTOT_PREFIX}_{FIRM}_{PERIOD}_STINVTOT iv
        INNER JOIN {DB_PREFIX}LG_{FIRM}_ITEMS it ON it.LOGICALREF = iv.STOCKREF
        INNER JOIN {DB_PREFIX}L_CAPIWHOUSE wh ON wh.NR = iv.INVENNO AND wh.FIRMNR = {FIRMNO}
        LEFT JOIN {DB_PREFIX}LG_{FIRM}_UNITSETL un ON un.UNITSETREF = it.UNITSETREF AND un.MAINUNIT = 1
        WHERE iv.INVENNO <> -1
        GROUP BY wh.NR, wh.NAME, it.CODE, it.NAME, un.CODE, it.ACTIVE
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

        var stinvtotPrefix = profile.StockSource == StockSourceKind.Table ? "LG" : "LV";
        var dbPrefix = string.IsNullOrWhiteSpace(profile.LinkedServerName)
            ? ""
            : $"[{profile.LinkedServerName}].[{profile.LinkedServerDatabase}].dbo.";

        var sql = template
            .Replace("{DB_PREFIX}", dbPrefix)
            .Replace("{STINVTOT_PREFIX}", stinvtotPrefix)
            .Replace("{FIRM}", profile.FirmSuffix)
            .Replace("{PERIOD}", profile.PeriodSuffix)
            .Replace("{FIRMNO}", profile.FirmNo.ToString());

        SqlReadOnlyGuard.EnsureReadOnly(sql);
        return sql;
    }
}
