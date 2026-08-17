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
/// schema documented at
/// https://logoisortagim.com.tr/blog-logo-veritabani-tablolari.html:
/// LG_&lt;firm&gt;_&lt;period&gt;_STINVTOT ("Günlük malzeme ambar
/// toplamları" — daily material warehouse totals, confirmed period-bound),
/// LG_&lt;firm&gt;_ITEMS ("Malzemeler / stok kartları", confirmed
/// CARDTYPE: 1=Ticari mal, 2=Hammadde, 4=Mamul, 11=Hizmet and
/// ACTIVE: 0=Aktif, 1=Pasif), and L_CAPIDEF ("Kuruluş bilgileri / ambar"
/// per that source). Column-level detail for STINVTOT and L_CAPIDEF was
/// not published there, so their column names (STOCKREF/INVENNO/ONHAND/
/// RESERVED/ORDERED, NR/NAME/FIRMNR) are still our best-effort guess
/// following the same conventions as the documented tables — some Logo
/// docs instead name the warehouse table L_CAPIWHOUSE. Heavily customized
/// Logo installations can differ further, so every profile can override
/// this template via ConnectionProfile.CustomQueryTemplate — the
/// placeholders below are the only contract PaperStok relies on.
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
        INNER JOIN L_CAPIDEF wh ON wh.NR = iv.INVENNO AND wh.FIRMNR = {FIRMNO}
        LEFT JOIN LG_{FIRM}_UNITSETF un ON un.REF = it.UNITSETREF AND un.UINFO = 0
        WHERE it.CARDTYPE = 1 AND it.ACTIVE = 0
        GROUP BY wh.NR, wh.NAME, it.CODE, it.NAME, un.NAME
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
