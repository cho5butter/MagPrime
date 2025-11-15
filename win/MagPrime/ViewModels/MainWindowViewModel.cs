using System.Collections.ObjectModel;
using System.Linq;
using MagPrime.Configuration;
using MagPrime.Infrastructure;
using MagPrime.Services;
using Microsoft.Extensions.Logging;

namespace MagPrime.ViewModels;

public sealed class MainWindowViewModel : ObservableObject
{
    private readonly IWindowCatalogService _windowCatalog;
    private readonly ISettingsProvider _settingsProvider;
    private readonly BootstrapService _bootstrap;
    private readonly IUiDispatcher _dispatcher;
    private readonly ILogger<MainWindowViewModel> _logger;

    private bool _isBusy;
    private bool _serviceActive;
    private string _statusMessage = "初期化中...";
    private int _windowCount;

    public MainWindowViewModel(
        IWindowCatalogService windowCatalog,
        ISettingsProvider settingsProvider,
        BootstrapService bootstrap,
        IUiDispatcher dispatcher,
        ILogger<MainWindowViewModel> logger)
    {
        _windowCatalog = windowCatalog;
        _settingsProvider = settingsProvider;
        _bootstrap = bootstrap;
        _dispatcher = dispatcher;
        _logger = logger;
        _serviceActive = bootstrap.IsRunning;
    }

    public ObservableCollection<WindowListItem> Windows { get; } = new();

    public bool IsBusy
    {
        get => _isBusy;
        private set => SetProperty(ref _isBusy, value);
    }

    public bool ServiceActive
    {
        get => _serviceActive;
        private set => SetProperty(ref _serviceActive, value);
    }

    public string StatusMessage
    {
        get => _statusMessage;
        private set => SetProperty(ref _statusMessage, value);
    }

    public int WindowCount
    {
        get => _windowCount;
        private set => SetProperty(ref _windowCount, value);
    }

    public async Task InitializeAsync()
    {
        await SetServiceStateAsync(true).ConfigureAwait(false);
        await RefreshAsync().ConfigureAwait(false);
    }

    public async Task SetServiceStateAsync(bool isEnabled)
    {
        if (isEnabled)
        {
            if (!_bootstrap.IsRunning)
            {
                _bootstrap.Start();
            }

            ServiceActive = true;
            StatusMessage = "グローバルフックが稼働中です";
        }
        else
        {
            if (_bootstrap.IsRunning)
            {
                _bootstrap.Stop();
            }

            ServiceActive = false;
            StatusMessage = "サービスは停止しています";
            await _dispatcher.EnqueueAsync(() =>
            {
                Windows.Clear();
                WindowCount = 0;
            }).ConfigureAwait(false);
        }
    }

    public async Task RefreshAsync()
    {
        if (!_bootstrap.IsRunning)
        {
            StatusMessage = "サービスを有効にするとウィンドウが表示されます";
            return;
        }

        IsBusy = true;
        try
        {
            var windows = await _windowCatalog.GetWindowsAsync().ConfigureAwait(false);
            var maxItems = Math.Max(1, _settingsProvider.Current.MaxMenuItems);
            await _dispatcher.EnqueueAsync(() =>
            {
                Windows.Clear();
                foreach (var descriptor in windows.Take(maxItems))
                {
                    Windows.Add(new WindowListItem(descriptor));
                }

                WindowCount = windows.Count;
            }).ConfigureAwait(false);

            StatusMessage = windows.Count == 0
                ? "移動可能なウィンドウが見つかりませんでした"
                : $"{windows.Count} 個のウィンドウを監視中";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to refresh window catalog.");
            StatusMessage = "ウィンドウ情報の取得に失敗しました";
        }
        finally
        {
            IsBusy = false;
        }
    }
}
