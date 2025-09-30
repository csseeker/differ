using Differ.UI.ViewModels;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Differ.UI.Views;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    private bool _isSyncingScroll;

    public MainWindow(MainViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel ?? throw new ArgumentNullException(nameof(viewModel));
    }

    private void TreeView_ScrollChanged(object sender, ScrollChangedEventArgs e)
    {
        if (_isSyncingScroll)
            return;

        _isSyncingScroll = true;

        var sourceTreeView = sender as TreeView;
        var targetTreeView = sourceTreeView == LeftTreeView ? RightTreeView : LeftTreeView;

        var sourceScrollViewer = FindScrollViewer(sourceTreeView);
        var targetScrollViewer = FindScrollViewer(targetTreeView);

        if (sourceScrollViewer != null && targetScrollViewer != null)
        {
            targetScrollViewer.ScrollToVerticalOffset(sourceScrollViewer.VerticalOffset);
            targetScrollViewer.ScrollToHorizontalOffset(sourceScrollViewer.HorizontalOffset);
        }

        _isSyncingScroll = false;
    }

    private ScrollViewer? FindScrollViewer(DependencyObject? parent)
    {
        if (parent == null)
            return null;

        for (int i = 0; i < VisualTreeHelper.GetChildrenCount(parent); i++)
        {
            var child = VisualTreeHelper.GetChild(parent, i);
            if (child is ScrollViewer scrollViewer)
            {
                return scrollViewer;
            }

            var result = FindScrollViewer(child);
            if (result != null)
            {
                return result;
            }
        }
        return null;
    }
}