using System;
using Differ.UI.ViewModels;
using System.Windows;

namespace Differ.UI.Views;

/// <summary>
/// Interaction logic for FileDiffWindow.xaml
/// </summary>
public partial class FileDiffWindow : Window
{
    public FileDiffWindow(FileDiffViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel ?? throw new ArgumentNullException(nameof(viewModel));
    }

    protected override void OnClosed(EventArgs e)
    {
        base.OnClosed(e);

        if (DataContext is IDisposable disposable)
        {
            disposable.Dispose();
        }
    }

    public FileDiffViewModel ViewModel => (FileDiffViewModel)DataContext;
}
