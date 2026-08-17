/*
 * Copyright (c) 2026 PaperAxis. All rights reserved.
 * This file is part of PaperStok. Unauthorized copying, modification
 * or distribution of this file is strictly prohibited.
 */
using System.Windows;

namespace PaperStok.App.Views;

/// <summary>Small reusable "enter a name" prompt — WPF has no built-in InputBox.</summary>
public partial class TextInputWindow : Window
{
    public string Value { get; private set; } = "";

    public TextInputWindow(string title, string prompt, string defaultValue = "")
    {
        InitializeComponent();
        Title = title;
        PromptText.Text = prompt;
        ValueBox.Text = defaultValue;
        Loaded += (_, _) =>
        {
            ValueBox.Focus();
            ValueBox.SelectAll();
        };
    }

    private void Ok_Click(object sender, RoutedEventArgs e)
    {
        var text = ValueBox.Text.Trim();
        if (string.IsNullOrEmpty(text))
        {
            ErrorText.Text = "Bir ad girin.";
            ErrorText.Visibility = Visibility.Visible;
            return;
        }

        Value = text;
        DialogResult = true;
        Close();
    }

    private void Cancel_Click(object sender, RoutedEventArgs e)
    {
        DialogResult = false;
        Close();
    }
}
