using MailKit.Net.Smtp;

namespace Kiwi2Shop.Notifications
{
    public class EmailService : IEmailService

    {
        private SmtpClient _client;
        private readonly ILogger<EmailService> _logger;
        private readonly IConfiguration _configuration;
        public EmailService(ILogger<EmailService> logger, IConfiguration configuration)
        {
            _logger = logger;
            _configuration = configuration;
            _client = new SmtpClient();
            // Initialize SMTP client with configuration settings if needed
            _client.Connect(
                _configuration["Smtp:Host"],
                int.Parse(_configuration["Smtp:Port"]),
                false);//use SSL
        }

        public async Task SendWellcomeEmail(string to, string subject, string body)
        {
            using (var client = new SmtpClient())
            {
                var fromAddress = _configuration["Smtp:FromAddress"];
                var message = new MimeKit.MimeMessage();
                message.From.Add(new MimeKit.MailboxAddress("Kiwi2Shop", fromAddress));
                message.To.Add(new MimeKit.MailboxAddress("", to));
                message.Subject = subject;
                message.Body = new MimeKit.TextPart("plain")
                {
                    Text = body
                };
                await _client.SendAsync(message);
                _logger.LogInformation("Wellcome email sent to {Email}", to);
            }

            //// Simulate sending an email
            //Console.WriteLine($"Sending Email to: {to}");
            //Console.WriteLine($"Subject: {subject}");
            //Console.WriteLine($"Body: {body}");
        }

    }
}
