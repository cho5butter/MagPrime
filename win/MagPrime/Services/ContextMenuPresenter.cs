using System.Runtime.InteropServices;
using MagPrime.Configuration;
using MagPrime.Infrastructure;
using MagPrime.Interop;
using MagPrime.Models;
using Microsoft.Extensions.Logging;

namespace MagPrime.Services;

public sealed class ContextMenuPresenter : IContextMenuPresenter
{
    private readonly ISettingsProvider _settingsProvider;
    private readonly ILogger<ContextMenuPresenter> _logger;
    private readonly IUiDispatcher _dispatcher;

    public ContextMenuPresenter(
        ISettingsProvider settingsProvider,
        ILogger<ContextMenuPresenter> logger,
        IUiDispatcher dispatcher)
    {
        _settingsProvider = settingsProvider;
        _logger = logger;
        _dispatcher = dispatcher;
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

        return _dispatcher.EnqueueAsync(() =>
        {
            var ordered = ApplySort(windows);
            var limited = ordered.Take(_settingsProvider.Current.MaxMenuItems).ToList();
            var selection = ShowNativeMenu(request, limited);
            if (selection is null)
            {
                _logger.LogDebug("Context menu dismissed without selection.");
            }
            else
            {
                _logger.LogDebug("Window {Title} selected from context menu.", selection.Title);
            }

            return selection;
        });
    }

    private IEnumerable<WindowDescriptor> ApplySort(IReadOnlyList<WindowDescriptor> windows)
    {
        return _settingsProvider.Current.SortMode switch
        {
            "Alphabetic" => windows.OrderBy(w => w.Title, StringComparer.OrdinalIgnoreCase),
            _ => windows
        };
    }

    private WindowDescriptor? ShowNativeMenu(MenuRequest request, IReadOnlyList<WindowDescriptor> windows)
    {
        var menuHandle = NativeMethods.CreatePopupMenu();
        if (menuHandle == nint.Zero)
        {
            return null;
        }

        try
        {
            for (var index = 0; index < windows.Count; index++)
            {
                var descriptor = windows[index];
                var label = $"{descriptor.ProcessName} | {descriptor.Title}";
                var info = new NativeMethods.MenuItemInfo
                {
                    cbSize = (uint)Marshal.SizeOf<NativeMethods.MenuItemInfo>(),
                    fMask = NativeMethods.MIIM_DATA | NativeMethods.MIIM_ID | NativeMethods.MIIM_STRING,
                    wID = (uint)(index + 1),
                    dwItemData = descriptor.Handle,
                    dwTypeData = label,
                    cch = (uint)label.Length
                };

                NativeMethods.InsertMenuItem(menuHandle, (uint)index, true, ref info);
            }

            var selectedId = NativeMethods.TrackPopupMenuEx(
                menuHandle,
                NativeMethods.TPM_RIGHTBUTTON | NativeMethods.TPM_RETURNCMD,
                request.CursorPosition.X,
                request.CursorPosition.Y,
                NativeMethods.GetForegroundWindow(),
                nint.Zero);

            if (selectedId == 0)
            {
                return null;
            }

            var idx = (int)selectedId - 1;
            return idx >= 0 && idx < windows.Count ? windows[idx] : null;
        }
        finally
        {
            NativeMethods.DestroyMenu(menuHandle);
        }
    }
}
