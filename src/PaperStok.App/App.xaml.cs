/*
 * Copyright (c) 2026 PaperAxis. All rights reserved.
 * This file is part of PaperStok. Unauthorized copying, modification
 * or distribution of this file is strictly prohibited.
 */
using System.Windows;
using System.Windows.Threading;

namespace PaperStok.App;

public partial class App : Application
{
    private void Application_DispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
    {
        MessageBox.Show(
            $"Beklenmeyen bir hata oluştu ve işlem tamamlanamadı:\n\n{e.Exception.Message}\n\nPaperStok kapatılmadan devam edecek.",
            "PaperStok — Hata",
            MessageBoxButton.OK,
            MessageBoxImage.Error);
        e.Handled = true;
    }
}
