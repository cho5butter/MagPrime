using MagPrime.Models;

namespace MagPrime.Configuration;

public interface ISettingsProvider
{
    AppSettings Current { get; }
    Task<AppSettings> ReloadAsync(CancellationToken cancellationToken = default);
}
