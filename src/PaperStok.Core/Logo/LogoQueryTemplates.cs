/*
 * Copyright (c) 2026 PaperAxis. All rights reserved.
 * This file is part of PaperStok. Unauthorized copying, modification
 * or distribution of this file is strictly prohibited.
 */
using PaperStok.Core.Models;

namespace PaperStok.Core.Logo;

/// <summary>
/// Builds the SQL used to read warehouse stock totals from a Logo Tiger3
/// Enterprise database. Table/column names were cross-checked against two
/// written sources (https://logoisortagim.com.tr/blog-logo-veritabani-tablolari.html
/// and the field-level dump at https://github.com/ugurozpinar/Logo), and then
/// against a live customer database via a read-only sys.tables discovery
/// query run through PaperStok itself.
///
/// The warehouse table is L_CAPIWHOUSE — confirmed by that live-database
/// query. Both written sources instead named L_CAPIDEF ("Kuruluş bilgileri
/// (ambar)"), which turned out not to exist on this installation ("Invalid
/// object name 'L_CAPIDEF'"); L_CAPIWHOUSE was our original, pre-"correction"
/// guess. This is the strongest evidence tier PaperStok has — a real
/// database beats any documentation — but its own columns (NR/NAME/FIRMNR)
/// are still inferred by analogy with sibling L_CAPI* tables, not directly
/// confirmed at the column level.
///
/// Confirmed at the field level (from the GitHub dump):
/// - LG_&lt;firm&gt;_&lt;period&gt;_STINVTOT ("Günlük Malzeme Ambar Toplamı"):
///   STOCKREF, INVENNO (-1 means "all warehouses" — excluded below), ONHAND,
///   RESERVED all exist exactly as used here. There is no ORDERED column;
///   ACTPORDER ("Verilen Siparişler" / purchase orders placed) is the closest
///   fit for an "on order" figure and is what OnOrder maps to.
/// - LG_&lt;firm&gt;_ITEMS: CODE, NAME, CARDTYPE (1 = Ticari Mal, the filter
///   used below), ACTIVE (0 = Aktif) and UNITSETREF all confirmed.
/// - LG_&lt;firm&gt;_UNITSETF is the unit *set* header (no per-unit name);
///   the actual unit code/name lives on LG_&lt;firm&gt;_UNITSETL, one row per
///   unit in the set, joined by UNITSETREF with MAINUNIT flagging the item's
///   base unit.
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
