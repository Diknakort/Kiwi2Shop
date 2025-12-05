using Kiwi2Shop.identity.Dto.Auth;
using Kiwi2Shop.identity.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Kiwi2Shop.identity.Services
{
    public class AuthService : IAuthService
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IConfiguration _configuration;

        public AuthService(UserManager<IdentityUser> userManager, RoleManager<IdentityRole> roleManager, IConfiguration configuration)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _configuration = configuration;
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

            // Read JWT configuration (support both "JWT" and "Jwt" keys)
            var issuer = _configuration["JWT:Issuer"] ?? _configuration["Jwt:Issuer"];
            var audience = _configuration["JWT:Audience"] ?? _configuration["Jwt:Audience"];
            var secretKey = _configuration["JWT:SecretKey"] ?? _configuration["Jwt:SecretKey"];
            var expirationValue = _configuration["JWT:ExpirationMinutes"] ?? _configuration["Jwt:ExpirationMinutes"] ?? _configuration["Logging:JWT:ExpireMinutes"] ?? _configuration["Logging:JWT:ExpirationMinutes"];
            if (!int.TryParse(expirationValue, out var expirationMinutes)) expirationMinutes = 60;

            if (string.IsNullOrEmpty(secretKey))
            {
                // No secret key configured
                return null;
            }

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

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

            return new ResponseLogin
            {
                Token = tokenString,
                Expiration = tokenObj.ValidTo
            };
        }

        public async Task<bool> Register(string username, string password)
        {
            var result = await _userManager.CreateAsync(new IdentityUser
            {
                UserName = username.Split("@")[0],
                Email = username
            }, password);
            return result.Succeeded;
        }
    }
}
