using MagPrime.Configuration;
using MagPrime.Models;
using Microsoft.Extensions.Logging;

namespace MagPrime.Services;

public sealed partial class ContextMenuPresenter : IContextMenuPresenter
{
    private readonly ISettingsProvider _settingsProvider;
    private readonly ILogger<ContextMenuPresenter> _logger;

    public ContextMenuPresenter(ISettingsProvider settingsProvider, ILogger<ContextMenuPresenter> logger)
    {
        _settingsProvider = settingsProvider;
        _logger = logger;
    }

    public Task<WindowDescriptor?> RequestSelectionAsync(
        MenuRequest request,
        IReadOnlyList<WindowDescriptor> windows,
        CancellationToken cancellationToken = default)
    {
        if (windows.Count == 0)
        {
            return Task.FromResult<WindowDescriptor?>(null);
        }

        return RequestSelectionCoreAsync(request, windows, cancellationToken);
    }

    partial Task<WindowDescriptor?> RequestSelectionCoreAsync(
        MenuRequest request,
        IReadOnlyList<WindowDescriptor> windows,
        CancellationToken cancellationToken);
}
