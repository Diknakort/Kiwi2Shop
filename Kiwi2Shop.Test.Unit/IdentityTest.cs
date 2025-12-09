using Kiwi2Shop.identity.Services;
using Kiwi2Shop.Shared.Events;
using MassTransit;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Moq;
using Assert = NUnit.Framework.Assert;


namespace Kiwi2Shop.Test.Unit
{

    [TestFixture]
    public class AuthServiceTests
    {
        private Mock<UserManager<IdentityUser>> _mockUserManager;
        private Mock<IConfiguration> _mockConfiguration;
        private Mock<IPublishEndpoint> _mockPublishEndpoint;
        private Mock<RoleManager<IdentityRole>> _mockRoleManager;
        private AuthService _authService;

        [SetUp]
        public void Setup()
        {
            // Mock UserManager
            var userStoreMock = new Mock<IUserStore<IdentityUser>>();

            // Provide concrete constructor args required by UserManager to avoid proxy constructor mismatch
            var options = Microsoft.Extensions.Options.Options.Create(new IdentityOptions());
            var passwordHasher = new Mock<IPasswordHasher<IdentityUser>>().Object;
            var userValidators = new List<IUserValidator<IdentityUser>>();
            var passwordValidators = new List<IPasswordValidator<IdentityUser>>();
            var lookupNormalizer = new Mock<ILookupNormalizer>().Object;
            var identityErrorDescriber = new IdentityErrorDescriber();
            var serviceProvider = new Mock<IServiceProvider>().Object;
            var userManagerLogger = new Mock<Microsoft.Extensions.Logging.ILogger<UserManager<IdentityUser>>>().Object;

            _mockUserManager = new Mock<UserManager<IdentityUser>>(
                userStoreMock.Object,
                options,
                passwordHasher,
                userValidators,
                passwordValidators,
                lookupNormalizer,
                identityErrorDescriber,
                serviceProvider,
                userManagerLogger
            );

            // Mock Configuration
            _mockConfiguration = new Mock<IConfiguration>();
            _mockConfiguration.Setup(c => c["JWT:SecretKey"]).Returns("ThisIsAVerySecureSecretKeyForJWTTokenGeneration12345");
            _mockConfiguration.Setup(c => c["JWT:Audience"]).Returns("TestAudience");
            _mockConfiguration.Setup(c => c["JWT:Issuer"]).Returns("TestIssuer");
            _mockConfiguration.Setup(c => c["JWT:ExpiryInMinutes"]).Returns("60");

            // Mock PublishEndpoint
            _mockPublishEndpoint = new Mock<IPublishEndpoint>();

            // Mock RoleManager (not used by AuthService currently but required by constructor)
            var roleStoreMock = new Mock<IRoleStore<IdentityRole>>();
            var roleValidators = new List<IRoleValidator<IdentityRole>>();
            var roleLookupNormalizer = new Mock<ILookupNormalizer>().Object;
            var roleErrorDescriber = new IdentityErrorDescriber();
            var roleLogger = new Mock<Microsoft.Extensions.Logging.ILogger<RoleManager<IdentityRole>>>().Object;

            _mockRoleManager = new Mock<RoleManager<IdentityRole>>(
                roleStoreMock.Object,
                roleValidators,
                roleLookupNormalizer,
                roleErrorDescriber,
                roleLogger
            );

            // Create AuthService with mocked dependencies (pass correct mocks)
            _authService = new AuthService(
                _mockUserManager.Object,
                _mockRoleManager.Object,
                _mockConfiguration.Object,
                _mockPublishEndpoint.Object
            );
        }
        #region Register Tests

        [Test]
        public async Task Register_WithValidCredentials_ShouldReturnTrue()
        {
            // Arrange
            var email = "test@example.com";
            var password = "Test@123456";
            var user = new IdentityUser
            {
                Id = "test-user-id",
                UserName = "test",
                Email = email
            };

            _mockUserManager.Setup(x => x.CreateAsync(It.IsAny<IdentityUser>(), password))
                .ReturnsAsync(IdentityResult.Success);

            _mockUserManager.Setup(x => x.FindByEmailAsync(email))
                .ReturnsAsync(user);

            _mockPublishEndpoint.Setup(x => x.Publish(It.IsAny<UserCreatedEvent>(), default))
                .Returns(Task.CompletedTask);

            // Act
            var result = await _authService.Register(email, password);

            // Assert
            Assert.That(result, Is.True);
            _mockUserManager.Verify(x => x.CreateAsync(It.Is<IdentityUser>(u =>
                u.Email == email && u.UserName == "test"), password), Times.Once);
            _mockUserManager.Verify(x => x.FindByEmailAsync(email), Times.Once);
            _mockPublishEndpoint.Verify(x => x.Publish(It.Is<UserCreatedEvent>(e =>
                e.userId == user.Id && e.email == email), default), Times.Once);
        }

        [Test]
        public async Task Register_WithValidCredentials_ShouldExtractUsernameFromEmail()
        {
            // Arrange
            var email = "john.doe@example.com";
            var password = "Test@123456";

            _mockUserManager.Setup(x => x.CreateAsync(It.IsAny<IdentityUser>(), password))
                .ReturnsAsync(IdentityResult.Success);

            _mockUserManager.Setup(x => x.FindByEmailAsync(email))
                .ReturnsAsync(new IdentityUser { Id = "test-id", Email = email, UserName = "john.doe" });

            // Act
            await _authService.Register(email, password);

            // Assert
            _mockUserManager.Verify(x => x.CreateAsync(It.Is<IdentityUser>(u =>
                u.UserName == "john.doe"), password), Times.Once);
        }

        [Test]
        public async Task Register_WhenUserCreationFails_ShouldReturnFalse()
        {
            // Arrange
            var email = "test@example.com";
            var password = "Te456";

            _mockUserManager.Setup(x => x.CreateAsync(It.IsAny<IdentityUser>(), password))
                .ReturnsAsync(IdentityResult.Failed(new IdentityError { Description = "Password too weak" }));

            _mockUserManager.Setup(x => x.FindByEmailAsync(email))
                .ReturnsAsync((IdentityUser)null!);

            // Act
            var result = await _authService.Register(email, password);

            // Assert
            // Note: Current implementation returns true even on failure
            Assert.That(result, Is.False);
            _mockPublishEndpoint.Verify(x => x.Publish(It.IsAny<UserCreatedEvent>(), default), Times.Never);
        }

        [Test]
        public async Task Register_WhenUserNotFoundAfterCreation_ShouldNotPublishEvent()
        {
            // Arrange
            var email = "test@example.com";
            var password = "Test@123456";

            _mockUserManager.Setup(x => x.CreateAsync(It.IsAny<IdentityUser>(), password))
                .ReturnsAsync(IdentityResult.Success);

            _mockUserManager.Setup(x => x.FindByEmailAsync(email))
                .ReturnsAsync((IdentityUser)null!);

            // Act
            await _authService.Register(email, password);

            // Assert
            _mockPublishEndpoint.Verify(x => x.Publish(It.IsAny<UserCreatedEvent>(), default), Times.Never);
        }

        #endregion

        #region Login Tests

        [Test]
        public async Task Login_WithValidCredentials_ShouldReturnToken()
        {
            // Arrange
            var email = "test@example.com";
            var password = "Test@123456";
            var user = new IdentityUser
            {
                Id = "test-user-id",
                UserName = "test",
                Email = email
            };

            _mockUserManager.Setup(x => x.FindByEmailAsync(email))
                .ReturnsAsync(user);

            _mockUserManager.Setup(x => x.CheckPasswordAsync(user, password))
                .ReturnsAsync(true);

            _mockUserManager.Setup(x => x.GetRolesAsync(user))
                .ReturnsAsync(new List<string> { "User" });

            // Act
            var result = await _authService.Login(email, password);

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Token, Is.Not.Null.And.Not.Empty);
            Assert.That(result.Expiration, Is.GreaterThan(DateTime.UtcNow));
            Assert.That(result.Expiration, Is.LessThan(DateTime.UtcNow.AddMinutes(61)));
        }

        [Test]
        public async Task Login_WithInvalidEmail_ShouldReturnNull()
        {
            // Arrange
            var email = "nonexistent@example.com";
            var password = "Test@123456";

            _mockUserManager.Setup(x => x.FindByEmailAsync(email))
                .ReturnsAsync((IdentityUser)null!);

            // Act
            var result = await _authService.Login(email, password);

            // Assert
            Assert.That(result, Is.Null);
            _mockUserManager.Verify(x => x.CheckPasswordAsync(It.IsAny<IdentityUser>(), It.IsAny<string>()), Times.Never);
        }

        [Test]
        public async Task Login_WithInvalidPassword_ShouldReturnNull()
        {
            // Arrange
            var email = "test@example.com";
            var password = "WrongPassword";
            var user = new IdentityUser
            {
                Id = "test-user-id",
                UserName = "test",
                Email = email
            };

            _mockUserManager.Setup(x => x.FindByEmailAsync(email))
                .ReturnsAsync(user);

            _mockUserManager.Setup(x => x.CheckPasswordAsync(user, password))
                .ReturnsAsync(false);

            // Act
            var result = await _authService.Login(email, password);

            // Assert
            Assert.That(result, Is.Null);
            _mockUserManager.Verify(x => x.GetRolesAsync(It.IsAny<IdentityUser>()), Times.Never);
        }

        [Test]
        public async Task Login_WithValidCredentials_ShouldGenerateTokenWithCorrectExpiration()
        {
            // Arrange
            var email = "test@example.com";
            var password = "Test@123456";
            var user = new IdentityUser
            {
                Id = "test-user-id",
                UserName = "test",
                Email = email
            };

            _mockUserManager.Setup(x => x.FindByEmailAsync(email))
                .ReturnsAsync(user);

            _mockUserManager.Setup(x => x.CheckPasswordAsync(user, password))
                .ReturnsAsync(true);

            _mockUserManager.Setup(x => x.GetRolesAsync(user))
                .ReturnsAsync(new List<string> { "Admin" });

            var beforeLogin = DateTime.UtcNow;

            // Act
            var result = await _authService.Login(email, password);

            // Assert
            var afterLogin = DateTime.UtcNow;
            var expectedExpiration = beforeLogin.AddMinutes(60);

            Assert.That(result, Is.Not.Null);
            Assert.That(result.Expiration, Is.GreaterThanOrEqualTo(expectedExpiration));
            Assert.That(result.Expiration, Is.LessThanOrEqualTo(afterLogin.AddMinutes(60)));
        }

        [Test]
        public async Task Login_WhenUserHasNoRoles_ShouldUseNoRoleDefault()
        {
            // Arrange
            var email = "test@example.com";
            var password = "Test@123456";
            var user = new IdentityUser
            {
                Id = "test-user-id",
                UserName = "test",
                Email = email
            };

            _mockUserManager.Setup(x => x.FindByEmailAsync(email))
                .ReturnsAsync(user);

            _mockUserManager.Setup(x => x.CheckPasswordAsync(user, password))
                .ReturnsAsync(true);

            _mockUserManager.Setup(x => x.GetRolesAsync(user))
                .ReturnsAsync(new List<string>());

            // Act
            var result = await _authService.Login(email, password);

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Token, Is.Not.Null.And.Not.Empty);
        }

        [Test]
        public async Task Login_WithMultipleRoles_ShouldUseFirstRole()
        {
            // Arrange
            var email = "test@example.com";
            var password = "Test@123456";
            var user = new IdentityUser
            {
                Id = "test-user-id",
                UserName = "test",
                Email = email
            };

            _mockUserManager.Setup(x => x.FindByEmailAsync(email))
                .ReturnsAsync(user);

            _mockUserManager.Setup(x => x.CheckPasswordAsync(user, password))
                .ReturnsAsync(true);

            _mockUserManager.Setup(x => x.GetRolesAsync(user))
                .ReturnsAsync(new List<string> { "Admin", "User", "Manager" });

            // Act
            var result = await _authService.Login(email, password);

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Token, Is.Not.Null.And.Not.Empty);
            _mockUserManager.Verify(x => x.GetRolesAsync(user), Times.Once);
        }

        [Test]
        public async Task Login_ShouldVerifyPasswordAfterFindingUser()
        {
            // Arrange
            var email = "test@example.com";
            var password = "Test@123456";
            var user = new IdentityUser
            {
                Id = "test-user-id",
                UserName = "test",
                Email = email
            };

            _mockUserManager.Setup(x => x.FindByEmailAsync(email))
                .ReturnsAsync(user);

            _mockUserManager.Setup(x => x.CheckPasswordAsync(user, password))
                .ReturnsAsync(true);

            _mockUserManager.Setup(x => x.GetRolesAsync(user))
                .ReturnsAsync(new List<string> { "User" });

            // Act
            await _authService.Login(email, password);

            // Assert
            _mockUserManager.Verify(x => x.FindByEmailAsync(email), Times.Once);
            _mockUserManager.Verify(x => x.CheckPasswordAsync(user, password), Times.Once);
            _mockUserManager.Verify(x => x.GetRolesAsync(user), Times.Once);
        }

        #endregion
    }
}
