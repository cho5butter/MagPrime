using MagPrime.Models;

namespace MagPrime.ViewModels;

public sealed class WindowListItem
{
    public WindowListItem(WindowDescriptor descriptor)
    {
        Descriptor = descriptor;
        Title = descriptor.Title;
        Subtitle = descriptor.ProcessName;
        Status = descriptor.IsMinimized ? "最小化" : "表示中";
        Glyph = descriptor.IsMinimized ? "\uE73D" : "\uE8A7";
        Dimensions = descriptor.Bounds.Width > 0 && descriptor.Bounds.Height > 0
            ? $"{descriptor.Bounds.Width} x {descriptor.Bounds.Height}"
            : "サイズ情報なし";
    }

    public WindowDescriptor Descriptor { get; }
    public string Title { get; }
    public string Subtitle { get; }
    public string Status { get; }
    public string Glyph { get; }
    public string Dimensions { get; }
}
