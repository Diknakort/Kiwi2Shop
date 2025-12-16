using Kiwi2Shop.identity.Dto.Auth;
using Kiwi2Shop.identity.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using MassTransit;
using Kiwi2Shop.Shared.Events;

namespace Kiwi2Shop.identity.Services
{
    public class AuthService : IAuthService
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IConfiguration _configuration;
        private readonly IPublishEndpoint? _publishEndpoint;
        private readonly ILogger<AuthService> _logger;

        public AuthService(
            UserManager<IdentityUser> userManager, 
            RoleManager<IdentityRole> roleManager, 
            IConfiguration configuration, 
            IPublishEndpoint? publishEndpoint = null,
            ILogger<AuthService>? logger = null)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _configuration = configuration;
            _publishEndpoint = publishEndpoint;
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<ResponseLogin?> Login(string username, string password)
        {
            var user = await _userManager.FindByEmailAsync(username);

            if (user == null)
                return null;

            var isPasswordValid = await _userManager.CheckPasswordAsync(user, password);
            if (!isPasswordValid)
                return null;

            var roles = await _userManager.GetRolesAsync(user);

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id),
                new Claim(ClaimTypes.Name, user.UserName ?? username),
                new Claim(ClaimTypes.Email, user.Email ?? string.Empty)
            };

            foreach (var role in roles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
            }

            // Read JWT configuration
            var secretKey = _configuration["JWT:SecretKey"]
                ?? throw new InvalidOperationException("JWT:SecretKey configuration is missing");
            var issuer = _configuration["JWT:Issuer"]
                ?? throw new InvalidOperationException("JWT:Issuer configuration is missing");
            var audience = _configuration["JWT:Audience"]
                ?? throw new InvalidOperationException("JWT:Audience configuration is missing");
            
            var expirationValue = _configuration["JWT:ExpirationMinutes"] ?? "60";
            if (!int.TryParse(expirationValue, out var expirationMinutes)) 
                expirationMinutes = 60;

            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
            var creds = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddMinutes(expirationMinutes),
                Issuer = issuer,
                Audience = audience,
                SigningCredentials = creds
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            var tokenObj = tokenHandler.CreateToken(tokenDescriptor);
            var tokenString = tokenHandler.WriteToken(tokenObj);

            _logger.LogInformation("JWT token generated for user: {Username}", username);

            return new ResponseLogin
            {
                Token = tokenString,
                Expiration = tokenDescriptor.Expires ?? tokenObj.ValidTo
            };
        }

        public async Task<bool> Register(string username, string password)
        {
            var result = await _userManager.CreateAsync(new IdentityUser
            {
                UserName = username.Split("@")[0],
                Email = username
            }, password);

            if (!result.Succeeded)
            {
                _logger.LogWarning("Failed to register user: {Username}", username);
                return false;
            }

            // Try to find the created user and publish an event
            var createdUser = await _userManager.FindByEmailAsync(username);
            if (createdUser != null && _publishEndpoint != null)
            {
                var userCreatedEvent = new UserCreatedEvent(createdUser.Id, createdUser.Email!);
                await _publishEndpoint.Publish(userCreatedEvent);
                _logger.LogInformation("User registered and event published: {Username}", username);
            }

            return true;
        }
    }
}
