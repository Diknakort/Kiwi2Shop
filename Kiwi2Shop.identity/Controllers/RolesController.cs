using Kiwi2Shop.Identity.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.IO;

namespace Kiwi2Shop.identity.Controllers
{
    [Authorize(Roles = "Admin")]
    [ApiController]
    [Route("api/[controller]")]
    public class RolesController : ControllerBase
    {
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly ILogger<RolesController> _logger;

        public RolesController(
            RoleManager<IdentityRole> roleManager,
            UserManager<IdentityUser> userManager,
            ILogger<RolesController> logger)
        {
            _roleManager = roleManager;
            _userManager = userManager;
            _logger = logger;
        }

        /// <summary>
        /// Obtener todos los roles
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<List<RoleInfo>>> GetAllRoles()
        {
            var roles = await _roleManager.Roles
                .Select(r => new RoleInfo
                {
                    Id = r.Id,
                    Name = r.Name!
                })
                .ToListAsync();

            return Ok(roles);
        }

        /// <summary>
        /// Crear un nuevo rol
        /// </summary>
        [HttpPost]
        public async Task<ActionResult<RoleResponse>> CreateRole([FromBody] RoleRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.RoleName))
            {
                return BadRequest(new RoleResponse
                {
                    Success = false,
                    Message = "El nombre del rol es requerido",
                    Errors = new List<string> { "Nombre de rol vacío" }
                });
            }

            var roleExists = await _roleManager.RoleExistsAsync(request.RoleName);
            if (roleExists)
            {
                return BadRequest(new RoleResponse
                {
                    Success = false,
                    Message = "El rol ya existe",
                    Errors = new List<string> { $"El rol '{request.RoleName}' ya está registrado" }
                });
            }

            var role = new IdentityRole(request.RoleName);
            var result = await _roleManager.CreateAsync(role);

            if (!result.Succeeded)
            {
                return BadRequest(new RoleResponse
                {
                    Success = false,
                    Message = "Error al crear el rol",
                    Errors = result.Errors.Select(e => e.Description).ToList()
                });
            }

            _logger.LogInformation("Rol creado: {RoleName}", request.RoleName);

            return Ok(new RoleResponse
            {
                Success = true,
                Message = "Rol creado exitosamente",
                Role = new RoleInfo
                {
                    Id = role.Id,
                    Name = role.Name!
                }
            });
        }

        /// <summary>
        /// Eliminar un rol
        /// </summary>
        [HttpDelete("{roleName}")]
        public async Task<ActionResult<RoleResponse>> DeleteRole(string roleName)
        {
            var role = await _roleManager.FindByNameAsync(roleName);

            if (role == null)
            {
                return NotFound(new RoleResponse
                {
                    Success = false,
                    Message = "Rol no encontrado"
                });
            }

            var result = await _roleManager.DeleteAsync(role);

            if (!result.Succeeded)
            {
                return BadRequest(new RoleResponse
                {
                    Success = false,
                    Message = "Error al eliminar el rol",
                    Errors = result.Errors.Select(e => e.Description).ToList()
                });
            }

            _logger.LogInformation("Rol eliminado: {RoleName}", roleName);

            return Ok(new RoleResponse
            {
                Success = true,
                Message = "Rol eliminado exitosamente"
            });
        }

        /// <summary>
        /// Asignar rol a un usuario
        /// </summary>
        [HttpPost("assign")]
        public async Task<ActionResult<RoleResponse>> AssignRoleToUser([FromBody] AssignRoleRequest request)
        {
            var user = await _userManager.FindByIdAsync(request.UserId);
            if (user == null)
            {
                return NotFound(new RoleResponse
                {
                    Success = false,
                    Message = "Usuario no encontrado"
                });
            }

            var roleExists = await _roleManager.RoleExistsAsync(request.RoleName);
            if (!roleExists)
            {
                return NotFound(new RoleResponse
                {
                    Success = false,
                    Message = "Rol no encontrado"
                });
            }

            var isInRole = await _userManager.IsInRoleAsync(user, request.RoleName);
            if (isInRole)
            {
                return BadRequest(new RoleResponse
                {
                    Success = false,
                    Message = "El usuario ya tiene este rol"
                });
            }

            var result = await _userManager.AddToRoleAsync(user, request.RoleName);

            if (!result.Succeeded)
            {
                return BadRequest(new RoleResponse
                {
                    Success = false,
                    Message = "Error al asignar el rol",
                    Errors = result.Errors.Select(e => e.Description).ToList()
                });
            }

            _logger.LogInformation("Rol '{RoleName}' asignado al usuario {Email}", request.RoleName, user.Email);

            return Ok(new RoleResponse
            {
                Success = true,
                Message = $"Rol '{request.RoleName}' asignado exitosamente"
            });
        }

        /// <summary>
        /// Remover rol de un usuario
        /// </summary>
        [HttpPost("remove")]
        public async Task<ActionResult<RoleResponse>> RemoveRoleFromUser([FromBody] AssignRoleRequest request)
        {
            var user = await _userManager.FindByIdAsync(request.UserId);
            if (user == null)
            {
                return NotFound(new RoleResponse
                {
                    Success = false,
                    Message = "Usuario no encontrado"
                });
            }

            var isInRole = await _userManager.IsInRoleAsync(user, request.RoleName);
            if (!isInRole)
            {
                return BadRequest(new RoleResponse
                {
                    Success = false,
                    Message = "El usuario no tiene este rol"
                });
            }

            var result = await _userManager.RemoveFromRoleAsync(user, request.RoleName);

            if (!result.Succeeded)
            {
                return BadRequest(new RoleResponse
                {
                    Success = false,
                    Message = "Error al remover el rol",
                    Errors = result.Errors.Select(e => e.Description).ToList()
                });
            }

            _logger.LogInformation("Rol '{RoleName}' removido del usuario {Email}", request.RoleName, user.Email);

            return Ok(new RoleResponse
            {
                Success = true,
                Message = $"Rol '{request.RoleName}' removido exitosamente"
            });
        }

        /// <summary>
        /// Obtener roles de un usuario
        /// </summary>
        [HttpGet("user/{userId}")]
        public async Task<ActionResult<List<string>>> GetUserRoles(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return NotFound(new { message = "Usuario no encontrado" });
            }

            var roles = await _userManager.GetRolesAsync(user);
            return Ok(roles);
        }

        /// <summary>
        /// Obtener usuarios por rol
        /// </summary>
        [HttpGet("{roleName}/users")]
        public async Task<ActionResult<List<UserInfo>>> GetUsersInRole(string roleName)
        {
            var roleExists = await _roleManager.RoleExistsAsync(roleName);
            if (!roleExists)
            {
                return NotFound(new { message = "Rol no encontrado" });
            }

            var usersInRole = await _userManager.GetUsersInRoleAsync(roleName);

            var userInfos = usersInRole.Select(u => new UserInfo
            {
                Id = u.Id,
                Email = u.Email!,
                UserName = u.UserName
            }).ToList();

            return Ok(userInfos);
        }
    }
}
