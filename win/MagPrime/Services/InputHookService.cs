using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Runtime.InteropServices;
using MagPrime.Infrastructure;
using MagPrime.Interop;
using MagPrime.Models;
using Microsoft.Extensions.Logging;

namespace MagPrime.Services;

public sealed class InputHookService : IInputHookService
{
    private readonly ILogger<InputHookService> _logger;
    private readonly IUiDispatcher _dispatcher;
    private nint _hookHandle;
    private NativeMethods.LowLevelMouseProc? _hookCallback;
    private bool _isRunning;

    public InputHookService(ILogger<InputHookService> logger, IUiDispatcher dispatcher)
    {
        _logger = logger;
        _dispatcher = dispatcher;
    }

    public event EventHandler<MenuRequest>? MenuRequested;

    public void Start()
    {
        if (_isRunning)
        {
            return;
        }

        _hookCallback = HookCallback;
        _hookHandle = NativeMethods.SetWindowsHookEx(
            HookType.WH_MOUSE_LL,
            _hookCallback,
            NativeMethods.GetModuleHandle(Process.GetCurrentProcess().MainModule?.ModuleName),
            0);

        if (_hookHandle == nint.Zero)
        {
            throw new Win32Exception(Marshal.GetLastWin32Error(), "SetWindowsHookEx failed.");
        }

        _isRunning = true;
        _logger.LogInformation("InputHookService started.");
    }

    public void Stop()
    {
        if (!_isRunning)
        {
            return;
        }

        if (_hookHandle != nint.Zero)
        {
            NativeMethods.UnhookWindowsHookEx(_hookHandle);
            _hookHandle = nint.Zero;
        }

        _isRunning = false;
        _logger.LogInformation("InputHookService stopped.");
    }

    private nint HookCallback(int nCode, nint wParam, nint lParam)
    {
        if (nCode >= 0 && wParam == NativeMethods.WM_RBUTTONUP)
        {
            var hookStruct = Marshal.PtrToStructure<NativeMethods.Msllhookstruct>(lParam);
            if (hookStruct is not null)
            {
                var point = new Point(hookStruct.Value.Point.X, hookStruct.Value.Point.Y);
                var request = new MenuRequest(DateTime.UtcNow, point, hookStruct.Value.WindowHandle);
                // Fire and forget with error logging
                _ = Task.Run(async () =>
                {
                    try
                    {
                        await _dispatcher.EnqueueAsync(() => MenuRequested?.Invoke(this, request));
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Failed to dispatch menu request.");
                    }
                });
            }
        }

        return NativeMethods.CallNextHookEx(_hookHandle, nCode, wParam, lParam);
    }

    public void Dispose()
    {
        Stop();
        GC.SuppressFinalize(this);
    }
}
