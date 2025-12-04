using Kiwi2Shop.Notifications;
using Kiwi2Shop.Shared.Events;
using MassTransit;


internal class UserRegisteredConsumer : IConsumer<UserCreatedEvent>
{

    private ILogger<UserRegisteredConsumer> _logger;
    private IEmailService _emailService;


    public UserRegisteredConsumer(ILogger<UserRegisteredConsumer> logger, IEmailService emailService)
    {
        _logger = logger;
        _emailService = emailService;
    }

    public Task Consume(ConsumeContext<UserCreatedEvent> context)
    {
        var user = context.Message;

        _logger.LogInformation("New user registered: {UserId}, Email: {Email}", user.userId, user.email);

        // Proporcionar los tres argumentos requeridos: to, subject, body
        var subject = "Bienvenido a Kiwi2Shop";
        var body = $"Hola,\n\nGracias por registrarte en Kiwi2Shop. Tu usuario es: {user.email}.\n\n¡Bienvenido!";
        return _emailService.SendWellcomeEmail(user.email, subject, body);
    }
}