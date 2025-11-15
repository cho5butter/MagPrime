using Microsoft.UI.Dispatching;

namespace MagPrime.Infrastructure;

public interface IUiDispatcher
{
    DispatcherQueue Queue { get; }
    Task EnqueueAsync(Action action);
    Task<T> EnqueueAsync<T>(Func<T> action);
}
