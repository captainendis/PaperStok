/*
 * Copyright (c) 2026 PaperAxis. All rights reserved.
 * This file is part of PaperStok. Unauthorized copying, modification
 * or distribution of this file is strictly prohibited.
 */
using System.Diagnostics;
using System.Windows;
using System.Windows.Navigation;
using PaperStok.Core;

namespace PaperStok.App.Views;

public partial class AboutWindow : Window
{
    public AboutWindow()
    {
        InitializeComponent();
        VersionText.Text = $"Sürüm {AppInfo.Version}";
        CopyrightText.Text = AppInfo.CopyrightLine;
    }

    private void Hyperlink_RequestNavigate(object sender, RequestNavigateEventArgs e)
    {
        Process.Start(new ProcessStartInfo(e.Uri.AbsoluteUri) { UseShellExecute = true });
        e.Handled = true;
    }

    private void CloseButton_Click(object sender, RoutedEventArgs e) => Close();
}
