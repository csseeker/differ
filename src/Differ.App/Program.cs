using Differ.Core.Interfaces;
using Differ.Core.Services;
using Differ.UI.ViewModels;
using Differ.UI.Views;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Windows;

namespace Differ.App;

/// <summary>
/// Application entry point
/// </summary>
public static class Program
{
    /// <summary>
    /// Application entry point
    /// </summary>
    [STAThread]
    public static void Main()
    {
        var app = new DifferApp();
        app.Run();
    }
}
