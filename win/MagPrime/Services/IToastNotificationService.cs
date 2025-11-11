namespace MagPrime.Services;

public interface IToastNotificationService
{
    Task ShowAsync(string title, string message);
}
