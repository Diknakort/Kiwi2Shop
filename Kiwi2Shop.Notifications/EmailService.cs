using MailKit.Net.Smtp;

namespace Kiwi2Shop.Notifications
{
    public class EmailService : IEmailService
    {
        private readonly ILogger<EmailService> _logger;
        private readonly IConfiguration _configuration;

        public EmailService(ILogger<EmailService> logger, IConfiguration configuration)
        {
            _logger = logger;
            _configuration = configuration;
        }

        public async Task SendWelcomeEmail(string to, string subject, string body)
        {
            try
            {
                using (var client = new SmtpClient())
                {
                    var smtpHost = _configuration["Smtp:Host"];
                    var smtpPort = int.Parse(_configuration["Smtp:Port"] ?? "1025");
                    var fromAddress = _configuration["Smtp:FromAddress"] ?? "noreply@kiwi2shop.local";

                    await client.ConnectAsync(smtpHost, smtpPort, false);

                    var message = new MimeKit.MimeMessage();
                    message.From.Add(new MimeKit.MailboxAddress("Kiwi2Shop", fromAddress));
                    message.To.Add(new MimeKit.MailboxAddress("", to));
                    message.Subject = subject;
                    message.Body = new MimeKit.TextPart("plain")
                    {
                        Text = body
                    };

                    await client.SendAsync(message);
                    await client.DisconnectAsync(true);

                    _logger.LogInformation("Welcome email sent successfully to {Email}", to);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending welcome email to {Email}", to);
                throw;
            }
        }
    }
}
