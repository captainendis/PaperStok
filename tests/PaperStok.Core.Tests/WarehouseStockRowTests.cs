/*
 * Copyright (c) 2026 PaperAxis. All rights reserved.
 * This file is part of PaperStok. Unauthorized copying, modification
 * or distribution of this file is strictly prohibited.
 */
using PaperStok.Core.Models;
using Xunit;

namespace PaperStok.Core.Tests;

public class WarehouseStockRowTests
{
    [Fact]
    public void Available_IsOnHandMinusReserved()
    {
        var row = new WarehouseStockRow { OnHand = 100m, Reserved = 35m };

        Assert.Equal(65m, row.Available);
    }
}
