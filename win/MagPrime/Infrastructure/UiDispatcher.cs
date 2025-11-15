using Microsoft.UI.Dispatching;

namespace MagPrime.Infrastructure;

public sealed class UiDispatcher : IUiDispatcher
{
    public UiDispatcher(DispatcherQueue queue)
    {
        Queue = queue;
    }

    public DispatcherQueue Queue { get; }

    public Task EnqueueAsync(Action action)
    {
        var tcs = new TaskCompletionSource();
        if (!Queue.TryEnqueue(() => Execute(action, tcs)))
        {
            tcs.SetException(new InvalidOperationException("Failed to enqueue action on DispatcherQueue."));
        }

        return tcs.Task;
    }

    public Task<T> EnqueueAsync<T>(Func<T> action)
    {
        var tcs = new TaskCompletionSource<T>();
        if (!Queue.TryEnqueue(() => Execute(action, tcs)))
        {
            tcs.SetException(new InvalidOperationException("Failed to enqueue function on DispatcherQueue."));
        }

        return tcs.Task;
    }

    private static void Execute(Action action, TaskCompletionSource tcs)
    {
        try
        {
            action();
            tcs.SetResult();
        }
        catch (Exception ex)
        {
            tcs.SetException(ex);
        }
    }

    private static void Execute<T>(Func<T> action, TaskCompletionSource<T> tcs)
    {
        try
        {
            var result = action();
            tcs.SetResult(result);
        }
        catch (Exception ex)
        {
            tcs.SetException(ex);
        }
    }
}
