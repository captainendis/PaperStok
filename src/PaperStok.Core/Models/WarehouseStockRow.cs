/*
 * Copyright (c) 2026 PaperAxis. All rights reserved.
 * This file is part of PaperStok. Unauthorized copying, modification
 * or distribution of this file is strictly prohibited.
 */
namespace PaperStok.Core.Models;

/// <summary>Bir ambardaki bir stok kaleminin toplam durumu.</summary>
public sealed class WarehouseStockRow
{
    public int WarehouseNo { get; set; }
    public string WarehouseName { get; set; } = "";
    public string ItemCode { get; set; } = "";
    public string ItemName { get; set; } = "";
    public string Unit { get; set; } = "";
    public decimal OnHand { get; set; }
    public decimal Reserved { get; set; }
    public decimal OnOrder { get; set; }

    /// <summary>Kullanılabilir miktar: eldeki - rezerve edilen.</summary>
    public decimal Available => OnHand - Reserved;
}
