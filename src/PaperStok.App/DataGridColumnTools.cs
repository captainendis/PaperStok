/*
 * Copyright (c) 2026 PaperAxis. All rights reserved.
 * This file is part of PaperStok. Unauthorized copying, modification
 * or distribution of this file is strictly prohibited.
 */
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;

namespace PaperStok.App;

/// <summary>Column-related helpers shared by PaperStok's data grids: a
/// right-click show/hide menu and a one-shot "fit to content" action.</summary>
public static class DataGridColumnTools
{
    /// <summary>Builds a right-click "show/hide columns" context menu for a DataGrid,
    /// reflecting whatever columns it currently has. Call again after a grid's
    /// columns are rebuilt (e.g. a dynamically generated pivot table) so the
    /// menu picks up the new set.</summary>
    public static void AttachVisibilityMenu(DataGrid grid)
    {
        var menu = new ContextMenu();
        foreach (var column in grid.Columns)
        {
            var item = new MenuItem
            {
                Header = column.Header?.ToString() ?? "",
                IsCheckable = true,
                IsChecked = column.Visibility == Visibility.Visible
            };
            item.Click += (_, _) => column.Visibility = item.IsChecked ? Visibility.Visible : Visibility.Collapsed;
            menu.Items.Add(item);
        }
        grid.ContextMenu = menu;
    }

    /// <summary>One-shot auto-fit: sizes every column to its content once, then
    /// freezes that size as a normal fixed width the user can still drag-resize
    /// by hand. Doesn't keep auto-resizing afterward — call again to re-fit.</summary>
    public static void AutoFitColumns(DataGrid grid)
    {
        foreach (var column in grid.Columns)
            column.Width = DataGridLength.Auto;

        // DataGridColumn.ActualWidth only reflects the Auto measurement after a
        // layout pass runs, so read it back on a deferred dispatch instead of
        // synchronously right after setting Width.
        grid.Dispatcher.InvokeAsync(() =>
        {
            foreach (var column in grid.Columns)
                column.Width = new DataGridLength(column.ActualWidth);
        }, DispatcherPriority.Loaded);
    }
}
