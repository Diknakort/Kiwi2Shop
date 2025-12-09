using Kiwi2Shop.Notifications;
using MassTransit;
using UserRegisteredConsumer;

var builder = Host.CreateApplicationBuilder(args);

builder.AddServiceDefaults();
builder.Services.AddSingleton<IEmailService, EmailService>();

//// Register the Worker as a hosted service
//builder.Services.AddHostedService<Worker>();
// Configure MassTransit with RabbitMQ
builder.Services.AddMassTransit(x =>
{
    // Register consumers
    x.AddConsumer<UserRegisteredConsumerClass>();

    x.UsingRabbitMq((context, cfg) =>
    {
        // Use Aspire service discovery for RabbitMQ connection
        var configuration = context.GetRequiredService<IConfiguration>();
        var connectionString = configuration.GetConnectionString("rabbitmq");

        if (!string.IsNullOrEmpty(connectionString))
        {
            cfg.Host(new Uri(connectionString));
        }

        // Configure retry policy
        cfg.UseMessageRetry(r => r.Intervals(
            TimeSpan.FromSeconds(1),
            TimeSpan.FromSeconds(5),
            TimeSpan.FromSeconds(15),
            TimeSpan.FromSeconds(30)));

        // Configure endpoints for all consumers
        cfg.ConfigureEndpoints(context);
    });
});

var host = builder.Build();

host.Run();
