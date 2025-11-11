using System.Drawing;

namespace MagPrime.Models;

public sealed record WindowDescriptor(
    nint Handle,
    string ProcessName,
    string Title,
    Rectangle Bounds,
    bool IsMinimized);
