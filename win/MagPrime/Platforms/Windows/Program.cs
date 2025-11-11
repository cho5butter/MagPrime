using Microsoft.UI.Xaml;
using System.Runtime.Versioning;

namespace MagPrime.WinUI;

public static class Program
{
    [STAThread]
    [SupportedOSPlatform("windows10.0.19041.0")]
    private static void Main(string[] args)
    {
        MauiWinUIApplication.StartApplication(nameof(App), args);
    }
}
