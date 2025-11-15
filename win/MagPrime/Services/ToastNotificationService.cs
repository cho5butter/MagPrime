using System.Security;
using Microsoft.Extensions.Logging;
using Windows.Data.Xml.Dom;
using Windows.UI.Notifications;

namespace MagPrime.Services;

public sealed class ToastNotificationService : IToastNotificationService
{
    private readonly ILogger<ToastNotificationService> _logger;

    public ToastNotificationService(ILogger<ToastNotificationService> logger)
    {
        _logger = logger;
    }

    public Task ShowAsync(string title, string message)
    {
        return Task.Run(() =>
        {
            try
            {
                var toastXml = $"""
                    <toast>
                      <visual>
                        <binding template="ToastGeneric">
                          <text>{SecurityElement.Escape(title)}</text>
                          <text>{SecurityElement.Escape(message)}</text>
                        </binding>
                      </visual>
                    </toast>
                    """;

                var xmlDoc = new XmlDocument();
                xmlDoc.LoadXml(toastXml);
                var notification = new ToastNotification(xmlDoc);
                ToastNotificationManager.CreateToastNotifier("MagPrime").Show(notification);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to show toast notification.");
            }
        });
    }
}
