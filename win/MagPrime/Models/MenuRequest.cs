using System.Drawing;

namespace MagPrime.Models;

public sealed record MenuRequest(DateTime Timestamp, Point CursorPosition, nint SourceWindow);
