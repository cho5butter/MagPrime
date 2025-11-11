using MagPrime.Configuration;
using MagPrime.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace MagPrime;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>();

#if DEBUG
        builder.Logging.AddDebug();
#endif

        builder.Services.AddSingleton<BootstrapService>();
        builder.Services.AddSingleton<ISettingsProvider, SettingsProvider>();
        builder.Services.AddSingleton<IInputHookService, InputHookService>();
        builder.Services.AddSingleton<IContextMenuPresenter, ContextMenuPresenter>();
        builder.Services.AddSingleton<IWindowCatalogService, WindowCatalogService>();
        builder.Services.AddSingleton<IWindowMoverService, WindowMoverService>();
        builder.Services.AddSingleton<IToastNotificationService, ToastNotificationService>();

        return builder.Build();
    }
}
