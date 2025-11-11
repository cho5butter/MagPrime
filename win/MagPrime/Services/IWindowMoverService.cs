using MagPrime.Models;

namespace MagPrime.Services;

public interface IWindowMoverService
{
    Task MoveWindowAsync(WindowDescriptor descriptor, MenuRequest request, CancellationToken cancellationToken = default);
}
