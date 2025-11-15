using System.Diagnostics;
using System.Runtime.InteropServices;
using MagPrime.Configuration;
using MagPrime.Interop;
using MagPrime.Models;
using Microsoft.Extensions.Logging;

namespace MagPrime.Services;

public sealed class WindowCatalogService : IWindowCatalogService
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

    private IReadOnlyList<WindowDescriptor> EnumerateWindows(AppSettings settings)
    {
        var windows = new List<WindowDescriptor>();
        var excluded = new HashSet<string>(settings.ExcludedProcesses ?? Array.Empty<string>(), StringComparer.OrdinalIgnoreCase);
        var shellWindow = NativeMethods.GetShellWindow();

        NativeMethods.EnumWindows((hWnd, _) =>
        {
            if (hWnd == shellWindow || !NativeMethods.IsWindowVisible(hWnd))
            {
                return true;
            }

            var length = NativeMethods.GetWindowTextLength(hWnd);
            if (length == 0)
            {
                return true;
            }

            var builder = new System.Text.StringBuilder(length + 1);
            NativeMethods.GetWindowText(hWnd, builder, builder.Capacity);
            var title = builder.ToString();

            NativeMethods.GetWindowThreadProcessId(hWnd, out var processId);
            string processName;
            try
            {
                processName = Process.GetProcessById((int)processId).ProcessName;
            }
            catch (Exception ex)
            {
                _logger.LogDebug(ex, "Failed to resolve process for window {Handle}", hWnd);
                processName = "Unknown";
            }

            if (excluded.Contains(processName))
            {
                return true;
            }

            if (!NativeMethods.GetWindowRect(hWnd, out var rect))
            {
                return true;
            }

            var placement = new NativeMethods.WindowPlacement { length = (uint)Marshal.SizeOf<NativeMethods.WindowPlacement>() };
            var isMinimized = NativeMethods.GetWindowPlacement(hWnd, ref placement)
                && placement.showCmd == ShowWindowCommand.Minimized;

            windows.Add(new WindowDescriptor(
                hWnd,
                processName,
                title,
                rect.ToRectangle(),
                isMinimized));

            return true;
        }, nint.Zero);

        return windows;
    }
}
