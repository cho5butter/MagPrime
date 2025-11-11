using MagPrime.Models;

namespace MagPrime.Services;

public interface IWindowCatalogService
{
    Task<IReadOnlyList<WindowDescriptor>> GetWindowsAsync(CancellationToken cancellationToken = default);
}
