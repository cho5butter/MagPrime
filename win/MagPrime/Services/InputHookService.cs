using MagPrime.Models;
using Microsoft.Extensions.Logging;

namespace MagPrime.Services;

public sealed partial class InputHookService : IInputHookService
{
    private readonly ILogger<InputHookService> _logger;
    private bool _isRunning;

    public InputHookService(ILogger<InputHookService> logger)
    {
        _logger = logger;
    }

    public event EventHandler<MenuRequest>? MenuRequested;

    public void Start()
    {
        if (_isRunning)
        {
            return;
        }

        StartPlatformHook();
        _isRunning = true;
        _logger.LogInformation("InputHookService started.");
    }

    public void Stop()
    {
        if (!_isRunning)
        {
            return;
        }

        StopPlatformHook();
        _isRunning = false;
        _logger.LogInformation("InputHookService stopped.");
    }

    private void RaiseMenuRequest(MenuRequest request)
    {
        MenuRequested?.Invoke(this, request);
    }

    public void Dispose()
    {
        Stop();
    }

    partial void StartPlatformHook();
    partial void StopPlatformHook();
}
