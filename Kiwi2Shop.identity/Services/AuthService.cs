using GranadaShopClase.API.Identity.Dto.Auth;
using GranadaShopClase.API.Identity.Services;
using Microsoft.AspNetCore.Identity;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace Kiwi2Shop.API.Identity.Services
{
    public class AuthService : IAuthService
    {
        private UserManager<IdentityUser> _userManager;
        private RoleManager<IdentityRole> _roleManager;
        private readonly IConfiguration _configuration;

        public AuthService(UserManager<IdentityUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            _userManager = userManager;
            _roleManager = roleManager;

        }

        public async Task<ResponseLogin> Login(string username, string password, string token)
        {


            var roles = await _userManager.GetRolesAsync(await _userManager.FindByNameAsync(username));
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, username),
                //new Claim(ClaimTypes.NameIdentifier, useusernamer.id),
                //new Claim(ClaimTypes.Email, username.Email!),
                new Claim(ClaimTypes.Role, roles.FirstOrDefault() ?? "NoRole"),


            };
            // JWT token configuration
            var secretKey = _configuration["Jwt:SecretKey"];
            var issuer = _configuration["Jwt:Issuer"];
            var audience = _configuration["Jwt:Audience"];
            var expirationMinutes = int.Parse(_configuration["Jwt:ExpirationMinutes"]!);

            var key = System.Text.Encoding.ASCII.GetBytes(secretKey!);

            //var creds = Environment.GetEnvironmentVariable("JWT_KEY");
            var creds = new Microsoft.IdentityModel.Tokens.SymmetricSecurityKey(key);

            var authToken = new System.IdentityModel.Tokens.Jwt.JwtSecurityToken(
                issuer: issuer,
                audience: audience,
                expires: DateTime.UtcNow.AddMinutes(expirationMinutes),
                signingCredentials: new Microsoft.IdentityModel.Tokens.SigningCredentials(creds, Microsoft.IdentityModel.Tokens.SecurityAlgorithms.HmacSha256Signature)
            );

            var tokenHandler = new JwtSecurityTokenHandler();
            var tokenDescriptor = new Microsoft.IdentityModel.Tokens.SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[]
                {
                    new Claim(ClaimTypes.Name, username)
                }),
                Expires = DateTime.UtcNow.AddMinutes(expirationMinutes),
                Issuer = issuer,
                Audience = audience,
                SigningCredentials = new Microsoft.IdentityModel.Tokens.SigningCredentials(creds, Microsoft.IdentityModel.Tokens.SecurityAlgorithms.HmacSha256Signature)
            };
            var tokenObj = tokenHandler.CreateToken(tokenDescriptor);
            var tokenString = tokenHandler.WriteToken(tokenObj);

            var encryptedToken = new JwtSecurityTokenHandler().WriteToken(authToken); // Aquí puedes agregar lógica para encriptar el token si es necesario

            // claims , expiration, etc.




            var user = await _userManager.FindByEmailAsync(username);

            if (user == null)
            {
                return null;
            }
            var isPasswordValid = await _userManager.CheckPasswordAsync(user, password);
            if (!isPasswordValid)
            {
                return null;
            }

            return new ResponseLogin
            {
                Token = tokenString,
                Expiration = tokenObj.ValidTo
            };

        }
        //public async Task<IdentityUser> Login(string username, string password)
        //{
        //    var user = await _userManager.FindByEmailAsync(username);

        //    if (user == null)
        //    {
        //        return null;
        //    }
        //    var isPasswordValid = await _userManager.CheckPasswordAsync(user, password);
        //    if (!isPasswordValid)
        //    {
        //        return null;
        //    }
        //    return user;

        //}

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
