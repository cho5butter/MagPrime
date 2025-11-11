using MagPrime.Models;

namespace MagPrime.Services;

public interface IContextMenuPresenter
{
    Task<WindowDescriptor?> RequestSelectionAsync(
        MenuRequest request,
        IReadOnlyList<WindowDescriptor> windows,
        CancellationToken cancellationToken = default);
}
