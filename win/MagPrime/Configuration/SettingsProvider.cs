using System.Text.Json;
using MagPrime.Models;
using Microsoft.Extensions.Logging;

namespace MagPrime.Configuration;

public sealed class SettingsProvider : ISettingsProvider
{
    private readonly ILogger<SettingsProvider> _logger;
    private readonly string _settingsPath;
    private readonly SemaphoreSlim _gate = new(1, 1);
    private AppSettings _current = new();

    public SettingsProvider(ILogger<SettingsProvider> logger)
    {
        _logger = logger;
        _settingsPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "MagPrime",
            "settings.json");

        Directory.CreateDirectory(Path.GetDirectoryName(_settingsPath)!);
        LoadFromDisk();
    }

    public AppSettings Current => _current;

    public async Task<AppSettings> ReloadAsync(CancellationToken cancellationToken = default)
    {
        await _gate.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            LoadFromDisk();
            return _current;
        }
        finally
        {
            _gate.Release();
        }
    }

    private void LoadFromDisk()
    {
        try
        {
            if (!File.Exists(_settingsPath))
            {
                PersistDefaults();
                return;
            }

            var json = File.ReadAllText(_settingsPath);
            var settings = JsonSerializer.Deserialize<AppSettings>(json);
            if (settings is not null)
            {
                _current = settings;
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to read settings file. Falling back to defaults.");
            _current = new AppSettings();
        }
    }

    private void PersistDefaults()
    {
        try
        {
            var json = JsonSerializer.Serialize(_current, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(_settingsPath, json);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to persist default settings.");
        }
    }

    public void Dispose()
    {
        _gate.Dispose();
        GC.SuppressFinalize(this);
    }
}
