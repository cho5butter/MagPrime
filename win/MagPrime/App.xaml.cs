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

    public App()
    {
        InitializeComponent();

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

        var window = _host.Services.GetRequiredService<MainWindow>();
        window.Activate();
    }
}
