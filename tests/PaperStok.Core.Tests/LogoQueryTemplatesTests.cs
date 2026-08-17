/*
 * Copyright (c) 2026 PaperAxis. All rights reserved.
 * This file is part of PaperStok. Unauthorized copying, modification
 * or distribution of this file is strictly prohibited.
 */
using PaperStok.Core.Logo;
using PaperStok.Core.Models;
using Xunit;

namespace PaperStok.Core.Tests;

public class LogoQueryTemplatesTests
{
    [Fact]
    public void Build_ReplacesFirmAndPeriodPlaceholders()
    {
        var profile = new ConnectionProfile { FirmNo = 3, PeriodNo = 1 };

        var sql = LogoQueryTemplates.Build(profile);

        Assert.Contains("LV_003_01_STINVTOT", sql);
        Assert.Contains("LG_003_ITEMS", sql);
        Assert.Contains("wh.FIRMNR = 3", sql);
        Assert.Contains("AS IsActive", sql);
        Assert.DoesNotContain("{FIRM}", sql);
        Assert.DoesNotContain("{PERIOD}", sql);
        Assert.DoesNotContain("{FIRMNO}", sql);
        Assert.DoesNotContain("{STINVTOT_PREFIX}", sql);
        Assert.DoesNotContain("{DB_PREFIX}", sql);
        // No linked server configured: table references stay unqualified (local database).
        Assert.Contains("FROM LV_003_01_STINVTOT", sql);
    }

    [Fact]
    public void Build_QualifiesTablesWithLinkedServerWhenConfigured()
    {
        var profile = new ConnectionProfile
        {
            FirmNo = 3,
            PeriodNo = 1,
            LinkedServerName = "UZAK_SUNUCU",
            LinkedServerDatabase = "TIGER3"
        };

        var sql = LogoQueryTemplates.Build(profile);

        Assert.Contains("FROM [UZAK_SUNUCU].[TIGER3].dbo.LV_003_01_STINVTOT", sql);
        Assert.Contains("[UZAK_SUNUCU].[TIGER3].dbo.LG_003_ITEMS", sql);
        Assert.Contains("[UZAK_SUNUCU].[TIGER3].dbo.L_CAPIWHOUSE", sql);
        Assert.Contains("[UZAK_SUNUCU].[TIGER3].dbo.LG_003_UNITSETL", sql);
        Assert.DoesNotContain("{DB_PREFIX}", sql);
    }

    [Fact]
    public void Build_UsesTableInsteadOfViewWhenStockSourceIsTable()
    {
        var profile = new ConnectionProfile { FirmNo = 3, PeriodNo = 1, StockSource = StockSourceKind.Table };

        var sql = LogoQueryTemplates.Build(profile);

        Assert.Contains("LG_003_01_STINVTOT", sql);
        Assert.DoesNotContain("LV_003_01_STINVTOT", sql);
    }

    [Fact]
    public void Build_DoesNotHardcodeCardTypeOrActiveFilters()
    {
        var profile = new ConnectionProfile { FirmNo = 3, PeriodNo = 1 };

        var sql = LogoQueryTemplates.Build(profile);

        Assert.DoesNotContain("CARDTYPE", sql);
        Assert.DoesNotContain("WHERE it.ACTIVE", sql);
    }

    [Fact]
    public void Build_UsesCustomTemplateWhenProvided()
    {
        var profile = new ConnectionProfile
        {
            FirmNo = 7,
            PeriodNo = 2,
            CustomQueryTemplate = "SELECT * FROM LG_{FIRM}_{PERIOD}_STINVTOT WHERE FIRM = {FIRMNO}"
        };

        var sql = LogoQueryTemplates.Build(profile);

        Assert.Equal("SELECT * FROM LG_007_02_STINVTOT WHERE FIRM = 7", sql);
    }

    [Fact]
    public void Build_PadsFirmAndPeriodToFixedWidth()
    {
        var profile = new ConnectionProfile { FirmNo = 1, PeriodNo = 1 };

        Assert.Equal("001", profile.FirmSuffix);
        Assert.Equal("01", profile.PeriodSuffix);
    }
}
