/*
 * Copyright (c) 2026 PaperAxis. All rights reserved.
 * This file is part of PaperStok. Unauthorized copying, modification
 * or distribution of this file is strictly prohibited.
 */
using System.Globalization;
using System.Windows.Data;
using PaperStok.Core.Reporting;

namespace PaperStok.App.Converters;

/// <summary>
/// Reads one warehouse-group column's quantity off a <see cref="StockReportRow"/>.
/// Used because report columns are only known at run time (they're the user's
/// warehouse group names), so the DataGrid's columns are built in code-behind
/// with a Binding("."){Converter=...} per column instead of a fixed property
/// path — this sidesteps WPF's indexer PropertyPath syntax entirely, which
/// would otherwise break on a group name containing '[', ']' or '.'.
/// </summary>
public sealed class GroupQuantityConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is StockReportRow row && parameter is string groupName)
            return row.QuantityFor(groupName);
        return 0m;
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        throw new NotSupportedException();
}
