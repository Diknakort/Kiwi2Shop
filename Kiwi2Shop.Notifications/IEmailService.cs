
namespace Kiwi2Shop.Notifications
{
    public interface IEmailService
    {
        Task SendWellcomeEmail(string to, string subject, string body);
    }
}