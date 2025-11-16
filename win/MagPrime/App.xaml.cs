using MagPrime.Configuration;
using MagPrime.Infrastructure;
using MagPrime.Services;
using MagPrime.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.UI.Dispatching;

namespace MagPrime;

public partial class App : Application
{
    private readonly IHost _host;
    private Window? _mainWindow;

    public App()
    {
        InitializeComponent();
        UnhandledException += OnUnhandledException;

        var dispatcherQueue = DispatcherQueue.GetForCurrentThread()
            ?? throw new InvalidOperationException("DispatcherQueue is not available on the current thread.");

        _host = Host.CreateDefaultBuilder()
            .ConfigureLogging(logging =>
            {
#if DEBUG
                logging.AddDebug();
#endif
            })
            .ConfigureServices(services =>
            {
                services.AddSingleton<IUiDispatcher>(_ => new UiDispatcher(dispatcherQueue));
                services.AddSingleton(dispatcherQueue);

                services.AddSingleton<BootstrapService>();
                services.AddSingleton<ISettingsProvider, SettingsProvider>();
                services.AddSingleton<IInputHookService, InputHookService>();
                services.AddSingleton<IContextMenuPresenter, ContextMenuPresenter>();
                services.AddSingleton<IWindowCatalogService, WindowCatalogService>();
                services.AddSingleton<IWindowMoverService, WindowMoverService>();
                services.AddSingleton<IToastNotificationService, ToastNotificationService>();

                services.AddSingleton<MainWindowViewModel>();
                services.AddSingleton<MainWindow>();
            })
            .Build();
    }

    protected override void OnLaunched(LaunchActivatedEventArgs args)
    {
        _host.Start();

        // Start bootstrap service early to enable global hooks
        var bootstrap = _host.Services.GetRequiredService<BootstrapService>();
        bootstrap.Start();

        _mainWindow = _host.Services.GetRequiredService<MainWindow>();
        _mainWindow.Closed += OnMainWindowClosed;
        _mainWindow.Activate();
    }

    private void OnMainWindowClosed(object sender, WindowEventArgs args)
    {
        // Clean up resources when main window is closed
        var bootstrap = _host.Services.GetService<BootstrapService>();
        bootstrap?.Dispose();

        var settingsProvider = _host.Services.GetService<ISettingsProvider>();
        settingsProvider?.Dispose();

        _host.Dispose();
    }

    private void OnUnhandledException(object sender, Microsoft.UI.Xaml.UnhandledExceptionEventArgs e)
    {
        var logger = _host.Services.GetService<ILogger<App>>();
        logger?.LogCritical(e.Exception, "Unhandled exception occurred.");
        e.Handled = true;
    }
}
