#if WINDOWS
using System.Security;
using Windows.Data.Xml.Dom;
using Windows.UI.Notifications;

namespace MagPrime.Services;

public sealed partial class ToastNotificationService
{
    private partial Task ShowWindowsToastAsync(string title, string message)
    {
        return Task.Run(() =>
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
        });
    }
}
#endif
