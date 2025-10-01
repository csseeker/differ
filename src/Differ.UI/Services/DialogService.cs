using Differ.UI.Resources;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;

namespace Differ.UI.Services;

/// <summary>
/// Implementation of IDialogService using WPF MessageBox.
/// </summary>
public class DialogService : IDialogService
{
    private readonly Dispatcher _dispatcher;

    public DialogService()
    {
        _dispatcher = Application.Current?.Dispatcher ?? Dispatcher.CurrentDispatcher;
    }

    /// <inheritdoc/>
    public void ShowError(string message, string? title = null)
    {
        if (_dispatcher.CheckAccess())
        {
            MessageBox.Show(
                message,
                title ?? AppMessages.ApplicationErrorTitle,
                MessageBoxButton.OK,
                MessageBoxImage.Error);
        }
        else
        {
            _dispatcher.Invoke(() => ShowError(message, title));
        }
    }

    /// <inheritdoc/>
    public void ShowWarning(string message, string? title = null)
    {
        if (_dispatcher.CheckAccess())
        {
            MessageBox.Show(
                message,
                title ?? AppMessages.WarningTitle,
                MessageBoxButton.OK,
                MessageBoxImage.Warning);
        }
        else
        {
            _dispatcher.Invoke(() => ShowWarning(message, title));
        }
    }

    /// <inheritdoc/>
    public void ShowInformation(string message, string? title = null)
    {
        if (_dispatcher.CheckAccess())
        {
            MessageBox.Show(
                message,
                title ?? AppMessages.InformationTitle,
                MessageBoxButton.OK,
                MessageBoxImage.Information);
        }
        else
        {
            _dispatcher.Invoke(() => ShowInformation(message, title));
        }
    }

    /// <inheritdoc/>
    public bool ShowConfirmation(string message, string? title = null)
    {
        if (_dispatcher.CheckAccess())
        {
            var result = MessageBox.Show(
                message,
                title ?? AppMessages.InformationTitle,
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            return result == MessageBoxResult.Yes;
        }
        else
        {
            return _dispatcher.Invoke(() => ShowConfirmation(message, title));
        }
    }

    /// <inheritdoc/>
    public Task ShowErrorAsync(string message, string? title = null)
    {
        return _dispatcher.InvokeAsync(() => ShowError(message, title)).Task;
    }

    /// <inheritdoc/>
    public Task ShowWarningAsync(string message, string? title = null)
    {
        return _dispatcher.InvokeAsync(() => ShowWarning(message, title)).Task;
    }
}
