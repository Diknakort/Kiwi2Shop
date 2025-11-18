using Kiwi2Shop.identity.Models;
using Kiwi2Shop.Identity.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Swashbuckle.AspNetCore.SwaggerUI;

namespace Kiwi2Shop.Identity.Controllers
{

    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly ILogger<UsersController> _logger;

        public UsersController(
            UserManager<IdentityUser> userManager,
            ILogger<UsersController> logger)
        {
            _userManager = userManager;
            _logger = logger;
        }

        /// <summary>
        /// Obtener todos los usuarios
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<List<identity.Models.UserInfo>>> GetAllUsers()
        {
            var users = await _userManager.Users
                .Select(u => new Models.UserInfo
                {
                    Id = u.Id,
                    Email = u.Email!,
                    UserName = u.UserName
                })
                .ToListAsync();

            return Ok(users);
        }

        /// <summary>
        /// Obtener un usuario por ID
        /// </summary>
        [HttpGet("{id}")]
        public async Task<ActionResult<identity.Models.UserInfo>> GetUserById(string id)
        {
            var user = await _userManager.FindByIdAsync(id);

            if (user == null)
            {
                return NotFound(new { message = "Usuario no encontrado" });
            }

            return base.Ok(new identity.Models.UserInfo
            {
                Id = user.Id,
                Email = user.Email!,
                UserName = user.UserName
            });
        }

        /// <summary>
        /// Eliminar un usuario
        /// </summary>
        [HttpDelete("{id}")]
        public async Task<ActionResult<AuthResponse>> DeleteUser(string id)
        {
            var user = await _userManager.FindByIdAsync(id);

            if (user == null)
            {
                return NotFound(new AuthResponse
                {
                    Success = false,
                    Message = "Usuario no encontrado"
                });
            }

            var result = await _userManager.DeleteAsync(user);

            if (!result.Succeeded)
            {
                return BadRequest(new AuthResponse
                {
                    Success = false,
                    Message = "Error al eliminar el usuario",
                    Errors = result.Errors.Select(e => e.Description).ToList()
                });
            }

            _logger.LogInformation("Usuario eliminado: {Email}", user.Email);

            return Ok(new AuthResponse
            {
                Success = true,
                Message = "Usuario eliminado exitosamente"
            });
        }
    }
}
//                 ? "Cuenta bloqueada debido a múltiples intentos fallidos"
//                 : "Email o contraseña incorrectos";
//             return Unauthorized(new AuthResponse
//             {
//                 Success = false,
//                 Message = message,

//                 Errors = new List<string> { "Credenciales inválidas" }
//             });
//         }
//
//         _logger.LogInformation("Usuario autenticado: {Email}", request.Email);
//         return Ok(new AuthResponse
//         {
//             Success = true,
//             Message = "Inicio de sesión exitoso",
//             User = new UserInfo
//             {
//                 Id = user.Id,
//                 Email = user.Email!,
//                 UserName = user.UserName
//             }
//         });
//     }
// }
    
