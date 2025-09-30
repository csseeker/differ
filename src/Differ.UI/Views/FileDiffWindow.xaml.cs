using System;
using Differ.UI.ViewModels;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Differ.UI.Views;

/// <summary>
/// Interaction logic for FileDiffWindow.xaml
/// </summary>
public partial class FileDiffWindow : Window
{
    private ScrollViewer? _leftScrollViewer;
    private ScrollViewer? _rightScrollViewer;
    private bool _isSynchronizingScroll;

    public FileDiffWindow(FileDiffViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel ?? throw new ArgumentNullException(nameof(viewModel));
    }

    protected override void OnClosed(EventArgs e)
    {
        base.OnClosed(e);

        if (_leftScrollViewer is not null)
        {
            _leftScrollViewer.ScrollChanged -= LeftScrollViewer_ScrollChanged;
        }

        if (_rightScrollViewer is not null)
        {
            _rightScrollViewer.ScrollChanged -= RightScrollViewer_ScrollChanged;
        }

        if (DataContext is IDisposable disposable)
        {
            disposable.Dispose();
        }
    }

    public FileDiffViewModel ViewModel => (FileDiffViewModel)DataContext;

    private void LeftDiffList_Loaded(object sender, RoutedEventArgs e)
    {
        _leftScrollViewer ??= FindScrollViewer(sender as DependencyObject);

        if (_leftScrollViewer is not null)
        {
            _leftScrollViewer.ScrollChanged -= LeftScrollViewer_ScrollChanged;
            _leftScrollViewer.ScrollChanged += LeftScrollViewer_ScrollChanged;
        }
    }

    private void RightDiffList_Loaded(object sender, RoutedEventArgs e)
    {
        _rightScrollViewer ??= FindScrollViewer(sender as DependencyObject);

        if (_rightScrollViewer is not null)
        {
            _rightScrollViewer.ScrollChanged -= RightScrollViewer_ScrollChanged;
            _rightScrollViewer.ScrollChanged += RightScrollViewer_ScrollChanged;
        }
    }

    private void LeftScrollViewer_ScrollChanged(object sender, ScrollChangedEventArgs e)
    {
        if (_isSynchronizingScroll || _rightScrollViewer is null || Math.Abs(e.VerticalChange) < double.Epsilon)
        {
            return;
        }

        SynchronizeScroll(e.VerticalOffset, _rightScrollViewer);
    }

    private void RightScrollViewer_ScrollChanged(object sender, ScrollChangedEventArgs e)
    {
        if (_isSynchronizingScroll || _leftScrollViewer is null || Math.Abs(e.VerticalChange) < double.Epsilon)
        {
            return;
        }

        SynchronizeScroll(e.VerticalOffset, _leftScrollViewer);
    }

    private void SynchronizeScroll(double verticalOffset, ScrollViewer target)
    {
        if (Math.Abs(target.VerticalOffset - verticalOffset) < 0.5)
        {
            return;
        }

        try
        {
            _isSynchronizingScroll = true;
            target.ScrollToVerticalOffset(verticalOffset);
        }
        finally
        {
            _isSynchronizingScroll = false;
        }
    }

    private static ScrollViewer? FindScrollViewer(DependencyObject? root)
    {
        if (root is null)
        {
            return null;
        }

        if (root is ScrollViewer viewer)
        {
            return viewer;
        }

        for (var i = 0; i < VisualTreeHelper.GetChildrenCount(root); i++)
        {
            var child = VisualTreeHelper.GetChild(root, i);
            var result = FindScrollViewer(child);
            if (result is not null)
            {
                return result;
            }
        }

        return null;
    }
}
