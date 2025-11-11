namespace MagPrime.Models;

public sealed class AppSettings
{
    public bool LaunchOnStartup { get; set; } = true;
    public int MaxMenuItems { get; set; } = 12;
    public string SortMode { get; set; } = "Recent";
    public string[] ExcludedProcesses { get; set; } = Array.Empty<string>();
    public bool EnableRestore { get; set; } = false;
    public bool ShowToast { get; set; } = true;
}
