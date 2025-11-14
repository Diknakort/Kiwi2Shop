using Kiwi2Shop.Identity.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace Kiwi2Shop.Identity.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly UserManager<IdentityUser> _userManager;
    private readonly SignInManager<IdentityUser> _signInManager;
    private readonly ILogger<AuthController> _logger;

    public AuthController(
        UserManager<IdentityUser> userManager,
        SignInManager<IdentityUser> signInManager,
        ILogger<AuthController> logger)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _logger = logger;
    }

    /// <summary>
    /// Registrar un nuevo usuario
    /// </summary>
    [HttpPost("register")]
    public async Task<ActionResult<AuthResponse>> Register([FromBody] RegisterRequest request)
    {
        // Validar que las contraseñas coincidan
        if (request.Password != request.ConfirmPassword)
        {
            return BadRequest(new AuthResponse
            {
                Success = false,
                Message = "Las contraseñas no coinciden",
                Errors = new List<string> { "Las contraseñas no coinciden" }
            });
        }

        // Crear el usuario
        var user = new IdentityUser
        {
            UserName = request.UserName ?? request.Email,
            Email = request.Email
        };

        var result = await _userManager.CreateAsync(user, request.Password);

        if (!result.Succeeded)
        {
            return BadRequest(new AuthResponse
            {
                Success = false,
                Message = "Error al crear el usuario",
                Errors = result.Errors.Select(e => e.Description).ToList()
            });
        }

        _logger.LogInformation("Usuario creado: {Email}", request.Email);

        // Iniciar sesión automáticamente
        await _signInManager.SignInAsync(user, isPersistent: false);

        return Ok(new AuthResponse
        {
            Success = true,
            Message = "Usuario registrado exitosamente",
            User = new UserInfo
            {
                Id = user.Id,
                Email = user.Email!,
                UserName = user.UserName
            }
        });
    }

    /// <summary>
    /// Iniciar sesión
    /// </summary>
    [HttpPost("login")]
    public async Task<ActionResult<AuthResponse>> Login([FromBody] LoginRequest request)
    {
        var user = await _userManager.FindByEmailAsync(request.Email);

        if (user == null)
        {
            return Unauthorized(new AuthResponse
            {
                Success = false,
                Message = "Email o contraseña incorrectos",
                Errors = new List<string> { "Credenciales inválidas" }
            });
        }

        var result = await _signInManager.PasswordSignInAsync(
            user,
            request.Password,
            request.RememberMe,
            lockoutOnFailure: true);

        if (!result.Succeeded)
        {
            var message = result.IsLockedOut
                ? "Cuenta bloqueada. Intenta más tarde."
                : "Email o contraseña incorrectos";

            return Unauthorized(new AuthResponse
            {
                Success = false,
                Message = message,
                Errors = new List<string> { message }
            });
        }

        _logger.LogInformation("Usuario inició sesión: {Email}", request.Email);

        return Ok(new AuthResponse
        {
            Success = true,
            Message = "Inicio de sesión exitoso",
            User = new UserInfo
            {
                Id = user.Id,
                Email = user.Email!,
                UserName = user.UserName
            }
        });
    }

    /// <summary>
    /// Cerrar sesión
    /// </summary>
    [Authorize]
    [HttpPost("logout")]
    public async Task<ActionResult<AuthResponse>> Logout()
    {
        await _signInManager.SignOutAsync();

        _logger.LogInformation("Usuario cerró sesión");

        return Ok(new AuthResponse
        {
            Success = true,
            Message = "Sesión cerrada exitosamente"
        });
    }

    /// <summary>
    /// Obtener información del usuario actual
    /// </summary>
    [Authorize]
    [HttpGet("me")]
    public async Task<ActionResult<AuthResponse>> GetCurrentUser()
    {
        var user = await _userManager.GetUserAsync(User);

        if (user == null)
        {
            return Unauthorized(new AuthResponse
            {
                Success = false,
                Message = "Usuario no autenticado"
            });
        }

        return Ok(new AuthResponse
        {
            Success = true,
            Message = "Usuario autenticado",
            User = new UserInfo
            {
                Id = user.Id,
                Email = user.Email!,
                UserName = user.UserName
            }
        });
    }

    /// <summary>
    /// Cambiar contraseña
    /// </summary>
    [Authorize]
    [HttpPost("change-password")]
    public async Task<ActionResult<AuthResponse>> ChangePassword([FromBody] ChangePasswordRequest request)
    {
        if (request.NewPassword != request.ConfirmNewPassword)
        {
            return BadRequest(new AuthResponse
            {
                Success = false,
                Message = "Las contraseñas no coinciden",
                Errors = new List<string> { "Las contraseñas nuevas no coinciden" }
            });
        }

        var user = await _userManager.GetUserAsync(User);

        if (user == null)
        {
            return Unauthorized(new AuthResponse
            {
                Success = false,
                Message = "Usuario no autenticado"
            });
        }

        var result = await _userManager.ChangePasswordAsync(
            user,
            request.CurrentPassword,
            request.NewPassword);

        if (!result.Succeeded)
        {
            return BadRequest(new AuthResponse
            {
                Success = false,
                Message = "Error al cambiar la contraseña",
                Errors = result.Errors.Select(e => e.Description).ToList()
            });
        }

        _logger.LogInformation("Usuario cambió su contraseña: {Email}", user.Email);

        return Ok(new AuthResponse
        {
            Success = true,
            Message = "Contraseña cambiada exitosamente"
        });
    }
}