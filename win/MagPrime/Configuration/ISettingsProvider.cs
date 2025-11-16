using MagPrime.Models;

namespace MagPrime.Configuration;

public interface ISettingsProvider : IDisposable
{
    AppSettings Current { get; }
    Task<AppSettings> ReloadAsync(CancellationToken cancellationToken = default);
}
