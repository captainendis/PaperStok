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

        Assert.Contains("LG_003_01_STINVTOT", sql);
        Assert.Contains("LG_003_ITEMS", sql);
        Assert.Contains("wh.FIRMNR = 3", sql);
        Assert.DoesNotContain("{FIRM}", sql);
        Assert.DoesNotContain("{PERIOD}", sql);
        Assert.DoesNotContain("{FIRMNO}", sql);
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
