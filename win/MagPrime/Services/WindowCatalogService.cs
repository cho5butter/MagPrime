using MagPrime.Configuration;
using MagPrime.Models;
using Microsoft.Extensions.Logging;

namespace MagPrime.Services;

public sealed partial class WindowCatalogService : IWindowCatalogService
{
    private readonly ISettingsProvider _settingsProvider;
    private readonly ILogger<WindowCatalogService> _logger;

    public WindowCatalogService(ISettingsProvider settingsProvider, ILogger<WindowCatalogService> logger)
    {
        _settingsProvider = settingsProvider;
        _logger = logger;
    }

    public Task<IReadOnlyList<WindowDescriptor>> GetWindowsAsync(CancellationToken cancellationToken = default)
    {
        return Task.Run(() => EnumerateWindows(_settingsProvider.Current), cancellationToken);
    }

    partial IReadOnlyList<WindowDescriptor> EnumerateWindows(AppSettings settings);
}
