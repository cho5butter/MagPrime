using MagPrime.ViewModels;
using Microsoft.UI;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Windows.Graphics;
using Windows.UI;
using WinRT.Interop;

namespace MagPrime;

public sealed partial class MainWindow : Window
{
    public MainWindow(MainWindowViewModel viewModel)
    {
        InitializeComponent();
        ViewModel = viewModel;
        Title = "MagPrime Control Center";
        if (MicaBackdrop.IsSupported())
        {
            SystemBackdrop = new MicaBackdrop();
        }
        ExtendsContentIntoTitleBar = false;
        Loaded += OnLoaded;
    }

    public MainWindowViewModel ViewModel { get; }

    private async void OnLoaded(object sender, RoutedEventArgs e)
    {
        ApplyWindowStyling();
        await ViewModel.InitializeAsync();
    }

    private void ApplyWindowStyling()
    {
        var hwnd = WindowNative.GetWindowHandle(this);
        var windowId = Win32Interop.GetWindowIdFromWindow(hwnd);
        var appWindow = AppWindow.GetFromWindowId(windowId);
        appWindow.Resize(new SizeInt32(1024, 720));
        var titleBar = appWindow.TitleBar;
        titleBar.BackgroundColor = Color.FromArgb(255, 17, 24, 39);
        titleBar.ForegroundColor = Colors.White;
        titleBar.ButtonBackgroundColor = Colors.Transparent;
        titleBar.ButtonForegroundColor = Colors.White;
    }

    private async void ServiceToggle_Toggled(object sender, RoutedEventArgs e)
    {
        if (sender is not ToggleSwitch toggle)
        {
            return;
        }

        await ViewModel.SetServiceStateAsync(toggle.IsOn);
        if (toggle.IsOn != ViewModel.ServiceActive)
        {
            toggle.IsOn = ViewModel.ServiceActive;
        }
    }

    private async void RefreshButton_Click(object sender, RoutedEventArgs e)
    {
        await ViewModel.RefreshAsync();
    }
}
