using CoreManagerSP.API.CoreManager.Application.DTOs.Logeo;
using CoreManagerSP.API.CoreManager.Application.Interfaces.Auth;
using CoreManagerSP.API.CoreManager.Infrastructure.Configurations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace CoreManagerSP.API.CoreManager.API.Controllers
{
    [ApiController]
    [Route("api/auth/usuario")] 
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;
        private readonly CoreManagerDbContext _context;

        public AuthController(IAuthService authService, CoreManagerDbContext context)
        {
            _authService = authService;
            _context = context;
        }

        /// <summary>
        /// Inicia sesión como Usuario (rol Usuario).
        /// </summary>
        [AllowAnonymous]
        [HttpPost("login")]
        public async Task<ActionResult<LoginResponseDto>> Login([FromBody] LoginRequestDto dto)
        {
            var result = await _authService.LoginAsync(dto);
            if (result == null) return Unauthorized("Credenciales inválidas");
            return Ok(result);
        }

        /// <summary>
        /// Registra un nuevo Usuario (no administrador).
        /// </summary>
        [AllowAnonymous]
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterRequestDto dto)
        {
            var success = await _authService.RegistrarUsuarioAsync(dto);
            if (!success) return BadRequest("Ya existe un usuario con ese correo");
            return Ok("Registro exitoso");
        }

        /// <summary>
        /// Cierra la sesión del Usuario autenticado.
        /// </summary>
        [Authorize(Roles = "Usuario")]
        [HttpPost("logout")]
        public async Task<IActionResult> Logout()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var token = Request.Headers["Authorization"].ToString().Replace("Bearer ", "");

            var tokenDb = await _context.TokenUsuarios
                .FirstOrDefaultAsync(t => t.Token == token && t.UsuarioId == int.Parse(userId));

            if (tokenDb == null) return NotFound("Token no encontrado.");

            tokenDb.EstaActivo = false;
            await _context.SaveChangesAsync();

            return Ok("Sesión cerrada exitosamente.");
        }
    }
}
