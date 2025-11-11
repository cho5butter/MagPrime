#if WINDOWS
using System.Runtime.InteropServices;
using MagPrime.Models;
using MagPrime.Platforms.Windows.Interop;
using Microsoft.Maui.Dispatching;

namespace MagPrime.Services;

public sealed partial class ContextMenuPresenter
{
    partial Task<WindowDescriptor?> RequestSelectionCoreAsync(
        MenuRequest request,
        IReadOnlyList<WindowDescriptor> windows,
        CancellationToken cancellationToken)
    {
        return MainThread.InvokeOnMainThreadAsync(() =>
        {
            var ordered = ApplySort(windows);
            var limited = ordered.Take(_settingsProvider.Current.MaxMenuItems).ToList();
            return ShowNativeMenu(request, limited);
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
#endif
