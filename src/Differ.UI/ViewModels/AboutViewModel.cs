using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Diagnostics;
using System.Reflection;

namespace Differ.UI.ViewModels;

/// <summary>
/// ViewModel for the About dialog
/// </summary>
public partial class AboutViewModel : ObservableObject
{
    [ObservableProperty]
    private string _applicationName = "Differ";

    [ObservableProperty]
    private string _description = "A professional directory and file comparison tool for Windows";

    [ObservableProperty]
    private string _version = "0.1.0";

    [ObservableProperty]
    private string _copyright = "Copyright © 2025 csseeker";

    [ObservableProperty]
    private string _githubUrl = "https://github.com/csseeker/differ";

    public AboutViewModel()
    {
        // Get version from assembly
        var assembly = Assembly.GetExecutingAssembly();
        var version = assembly.GetName().Version;
        if (version != null)
        {
            Version = $"{version.Major}.{version.Minor}.{version.Build}";
        }
    }

    [RelayCommand]
    private void OpenGitHub()
    {
        try
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = GithubUrl,
                UseShellExecute = true
            });
        }
        catch (Exception)
        {
            // Silently fail if browser can't be opened
        }
    }
}
