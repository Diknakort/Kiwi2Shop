using Microsoft.AspNetCore.Identity;

namespace GranadaShopClase.API.Identity.Services
{
    public interface IAuthService
    {
        Task<bool> Register(string username, string password);
        //Task<IdentityUser> Login(string username, string password);
    }
}
