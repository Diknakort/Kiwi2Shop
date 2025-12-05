using Kiwi2Shop.identity.Dto.Auth;

namespace Kiwi2Shop.identity.Services
{
    public interface IAuthService
    {
        Task<bool> Register(string username, string password);
        Task<ResponseLogin?> Login(string username, string password);
        //Task<IdentityUser> Login(string username, string password);
    }
}
