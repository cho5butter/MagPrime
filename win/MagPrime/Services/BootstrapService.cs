using MagPrime.Models;
using Microsoft.Extensions.Logging;

namespace MagPrime.Services;

public sealed class BootstrapService : IDisposable
{
    private readonly IInputHookService _hookService;
    private readonly IWindowCatalogService _windowCatalog;
    private readonly IContextMenuPresenter _menuPresenter;
    private readonly IWindowMoverService _windowMover;
    private readonly IToastNotificationService _toastNotification;
    private readonly ILogger<BootstrapService> _logger;
    private bool _isStarted;

    public BootstrapService(
        IInputHookService hookService,
        IWindowCatalogService windowCatalog,
        IContextMenuPresenter menuPresenter,
        IWindowMoverService windowMover,
        IToastNotificationService toastNotification,
        ILogger<BootstrapService> logger)
    {
        _hookService = hookService;
        _windowCatalog = windowCatalog;
        _menuPresenter = menuPresenter;
        _windowMover = windowMover;
        _toastNotification = toastNotification;
        _logger = logger;
    }

    public void Start()
    {
        if (_isStarted)
        {
            return;
        }

        _hookService.MenuRequested += HandleMenuRequestedAsync;
        _hookService.Start();
        _isStarted = true;
        _logger.LogInformation("BootstrapService started background hooks.");
    }

    private async void HandleMenuRequestedAsync(object? sender, MenuRequest request)
    {
        try
        {
            var windows = await _windowCatalog.GetWindowsAsync().ConfigureAwait(false);
            if (windows.Count == 0)
            {
                await _toastNotification.ShowAsync("MagPrime", "移動可能なウィンドウが見つかりませんでした。")
                    .ConfigureAwait(false);
                return;
            }

            var selection = await _menuPresenter.RequestSelectionAsync(request, windows).ConfigureAwait(false);
            if (selection is null)
            {
                _logger.LogDebug("User dismissed menu.");
                return;
            }

            await _windowMover.MoveWindowAsync(selection, request).ConfigureAwait(false);
            await _toastNotification.ShowAsync("MagPrime", $"{selection.Title} を現在の画面へ移動しました。")
                .ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to handle menu request.");
            await _toastNotification.ShowAsync("MagPrime", "ウィンドウ移動に失敗しました。ログを確認してください。")
                .ConfigureAwait(false);
        }
    }

    public void Dispose()
    {
        _hookService.MenuRequested -= HandleMenuRequestedAsync;
        _hookService.Dispose();
    }
}
