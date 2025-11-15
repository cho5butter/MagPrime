using System.Drawing;
using System.Runtime.InteropServices;
using MagPrime.Interop;
using MagPrime.Models;

namespace MagPrime.Services;

public sealed class WindowMoverService : IWindowMoverService
{
    public Task MoveWindowAsync(WindowDescriptor descriptor, MenuRequest request, CancellationToken cancellationToken = default)
    {
        return Task.Run(() =>
        {
            var targetMonitor = NativeMethods.MonitorFromPoint(
                new Point(request.CursorPosition.X, request.CursorPosition.Y),
                MonitorDefaultFlags.MONITOR_DEFAULTTONEAREST);

            var info = new NativeMethods.MonitorInfo
            {
                cbSize = (uint)Marshal.SizeOf<NativeMethods.MonitorInfo>()
            };

            if (targetMonitor == nint.Zero || !NativeMethods.GetMonitorInfo(targetMonitor, ref info))
            {
                return;
            }

            var workArea = info.rcWork;
            var width = descriptor.Bounds.Width > 0 ? descriptor.Bounds.Width : 800;
            var height = descriptor.Bounds.Height > 0 ? descriptor.Bounds.Height : 600;
            var targetX = workArea.Left + ((workArea.Right - workArea.Left - width) / 2);
            var targetY = workArea.Top + ((workArea.Bottom - workArea.Top - height) / 2);

            NativeMethods.ShowWindow(descriptor.Handle, NativeMethods.SW_RESTORE);
            NativeMethods.SetWindowPos(
                descriptor.Handle,
                NativeMethods.HWND_TOPMOST,
                targetX,
                targetY,
                width,
                height,
                NativeMethods.SWP_SHOWWINDOW);
            NativeMethods.SetWindowPos(
                descriptor.Handle,
                NativeMethods.HWND_NOTOPMOST,
                targetX,
                targetY,
                width,
                height,
                NativeMethods.SWP_NOACTIVATE);

            var foreground = NativeMethods.GetForegroundWindow();
            var targetThread = NativeMethods.GetWindowThreadProcessId(descriptor.Handle, out _);
            uint currentThread = 0;
            if (foreground != nint.Zero)
            {
                currentThread = NativeMethods.GetWindowThreadProcessId(foreground, out _);
            }

            var shouldAttach = currentThread != 0 && currentThread != targetThread;
            if (shouldAttach)
            {
                NativeMethods.AttachThreadInput(currentThread, targetThread, true);
            }

            NativeMethods.SetForegroundWindow(descriptor.Handle);

            if (shouldAttach)
            {
                NativeMethods.AttachThreadInput(currentThread, targetThread, false);
            }
        }, cancellationToken);
    }
}
