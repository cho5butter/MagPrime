using Microsoft.Extensions.Logging;

namespace MagPrime.Services;

public sealed partial class ToastNotificationService : IToastNotificationService
{
    private readonly ILogger<ToastNotificationService> _logger;

    public ToastNotificationService(ILogger<ToastNotificationService> logger)
    {
        _logger = logger;
    }

    public Task ShowAsync(string title, string message)
    {
#if WINDOWS
        return ShowWindowsToastAsync(title, message);
#else
        _logger.LogInformation("{Title}: {Message}", title, message);
        return Task.CompletedTask;
#endif
    }

#if !WINDOWS
    private Task ShowWindowsToastAsync(string title, string message) => Task.CompletedTask;
#else
    private partial Task ShowWindowsToastAsync(string title, string message);
#endif
}
