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
        appWindow.Resize(new SizeInt32(1100, 780));

        // Modern Title Bar with System Theme Colors
        var titleBar = appWindow.TitleBar;
        titleBar.BackgroundColor = Colors.Transparent;
        titleBar.ForegroundColor = Colors.White;
        titleBar.ButtonBackgroundColor = Colors.Transparent;
        titleBar.ButtonForegroundColor = Colors.White;
        titleBar.ButtonHoverBackgroundColor = Color.FromArgb(32, 255, 255, 255);
        titleBar.ButtonHoverForegroundColor = Colors.White;
        titleBar.ButtonPressedBackgroundColor = Color.FromArgb(16, 255, 255, 255);
        titleBar.ButtonPressedForegroundColor = Colors.White;
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

    private void WindowItem_PointerEntered(object sender, Microsoft.UI.Xaml.Input.PointerRoutedEventArgs e)
    {
        if (sender is Border border)
        {
            border.Background = Resources["CardHoverBrush"] as Brush;
            border.Scale = new System.Numerics.Vector3(1.02f, 1.02f, 1.0f);
        }
    }

    private void WindowItem_PointerExited(object sender, Microsoft.UI.Xaml.Input.PointerRoutedEventArgs e)
    {
        if (sender is Border border)
        {
            border.Background = Resources["LayerFillColorDefaultBrush"] as Brush
                ?? new SolidColorBrush(Color.FromArgb(255, 31, 41, 55));
            border.Scale = System.Numerics.Vector3.One;
        }
    }
}
