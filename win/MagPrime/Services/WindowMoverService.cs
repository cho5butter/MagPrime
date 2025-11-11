using MagPrime.Models;

namespace MagPrime.Services;

public sealed partial class WindowMoverService : IWindowMoverService
{
    public Task MoveWindowAsync(WindowDescriptor descriptor, MenuRequest request, CancellationToken cancellationToken = default)
    {
        return MoveWindowAsyncInternal(descriptor, request, cancellationToken);
    }

    partial Task MoveWindowAsyncInternal(WindowDescriptor descriptor, MenuRequest request, CancellationToken cancellationToken);
}
