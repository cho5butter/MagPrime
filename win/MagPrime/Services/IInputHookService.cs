using MagPrime.Models;

namespace MagPrime.Services;

public interface IInputHookService : IDisposable
{
    event EventHandler<MenuRequest>? MenuRequested;
    void Start();
    void Stop();
}
