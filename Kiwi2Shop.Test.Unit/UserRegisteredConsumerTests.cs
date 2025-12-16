using System.Threading.Tasks;
using Moq;
using Microsoft.Extensions.Logging;
using MassTransit;
using Kiwi2Shop.Notifications;
using Kiwi2Shop.Shared.Events;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.Json;
//using Kiwi2Shop.ProductsAPI.Services;
using Kiwi2Shop.Shared.Services;
using Kiwi2Shop.Shared.Dto;
using NUnit.Framework;
using Assert = NUnit.Framework.Assert;
using UserRegisteredConsumer; // Asegúrate de que esta directiva esté presente

namespace Kiwi2Shop.Test.Unit
{
    [TestFixture]
    public sealed class UserRegisteredConsumerTests
    {
        [Test]
        public async Task Consume_CallsSendWellcomeEmail_WithExpectedArguments()
        {
            // Arrange
            var mockEmail = new Mock<IEmailService>();
            mockEmail
                .Setup(m => m.SendWelcomeEmail(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                .Returns(Task.CompletedTask);

            var mockLogger = new Mock<ILogger<UserRegisteredConsumerClass>>();

            var consumer = new UserRegisteredConsumerClass(mockLogger.Object, mockEmail.Object);

            var userEvent = new UserCreatedEvent("user-123", "user@example.com");

            var mockContext = new Mock<ConsumeContext<UserCreatedEvent>>();
            mockContext.SetupGet(c => c.Message).Returns(userEvent);

            // Act
            await consumer.Consume(mockContext.Object);

            // Assert
            mockEmail.Verify(m => m.SendWelcomeEmail(
                It.Is<string>(s => s == userEvent.email),
                It.Is<string>(sub => sub == "Bienvenido a Kiwi2Shop"),
                It.Is<string>(body => body != null && body.Contains(userEvent.email))
            ), Times.Once);
        }
    }
}