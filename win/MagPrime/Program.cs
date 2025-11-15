using Microsoft.UI.Xaml;
using WinRT;

namespace MagPrime;

public static class Program
{
    [STAThread]
    public static void Main(string[] args)
    {
        ComWrappersSupport.InitializeComWrappers();
        Application.Start(_ => new App());
    }
}
