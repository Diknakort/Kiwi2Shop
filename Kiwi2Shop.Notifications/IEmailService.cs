namespace Kiwi2Shop.Notifications
{
    public interface IEmailService
    {
        Task SendWelcomeEmail(string to, string subject, string body);
    }
}