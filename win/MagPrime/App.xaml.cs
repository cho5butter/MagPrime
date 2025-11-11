using MagPrime.Services;

namespace MagPrime;

public partial class App : Application
{
    private readonly BootstrapService _bootstrap;

    public App(BootstrapService bootstrap)
    {
        InitializeComponent();
        _bootstrap = bootstrap;
    }

    protected override Window CreateWindow(IActivationState? activationState)
    {
        _bootstrap.Start();

        // Window remains hidden; we still return a minimal surface to satisfy MAUI requirements.
        var placeholderPage = new ContentPage
        {
            Content = new Label
            {
                Text = "MagPrime background service running.",
                IsVisible = false
            }
        };

        var window = new Window(placeholderPage)
        {
            Title = "MagPrime",
            Width = 1,
            Height = 1,
            X = -32000,
            Y = -32000
        };

        return window;
    }
}
