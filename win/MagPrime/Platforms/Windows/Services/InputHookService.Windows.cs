#if WINDOWS
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Runtime.InteropServices;
using MagPrime.Models;
using MagPrime.Platforms.Windows.Interop;
using Microsoft.Maui.Dispatching;

namespace MagPrime.Services;

public sealed partial class InputHookService
{
    private nint _hookHandle;
    private NativeMethods.LowLevelMouseProc? _hookCallback;

    partial void StartPlatformHook()
    {
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
    }

    partial void StopPlatformHook()
    {
        if (_hookHandle == nint.Zero)
        {
            return;
        }

        NativeMethods.UnhookWindowsHookEx(_hookHandle);
        _hookHandle = nint.Zero;
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
                MainThread.BeginInvokeOnMainThread(() => RaiseMenuRequest(request));
            }
        }

        return NativeMethods.CallNextHookEx(_hookHandle, nCode, wParam, lParam);
    }
}
#endif
