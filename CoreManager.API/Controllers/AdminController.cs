using CoreManagerSP.API.CoreManager.Application.DTOs.Logeo;
using CoreManagerSP.API.CoreManager.Application.DTOs.Usuarios;
using CoreManagerSP.API.CoreManager.Application.Interfaces.Usuarios;
using CoreManagerSP.API.CoreManager.Infrastructure.Configurations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace CoreManagerSP.API.CoreManager.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AdminController : ControllerBase
    {
        private readonly IAdminService _adminService;
        private readonly CoreManagerDbContext _context;

        public AdminController(IAdminService adminService, CoreManagerDbContext context)
        {
            _adminService = adminService;
            _context = context;
        }

        // ──────────────────────────────────────────────────────────────
        // ENDPOINTS ACTIVOS (Login / Logout / Registro)
        // ──────────────────────────────────────────────────────────────

        [AllowAnonymous]
        [HttpPost("login")]
        public async Task<ActionResult<AdminLoginResponseDto>> Login([FromBody] AdminLoginDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (string.IsNullOrWhiteSpace(dto.Correo) || string.IsNullOrWhiteSpace(dto.Contrasena))
                return BadRequest(new { mensaje = "Correo y contraseña son obligatorios." });

            var result = await _adminService.LoginAsync(dto);
            if (result == null)
                return Unauthorized(new { mensaje = "Credenciales inválidas." });

            return Ok(result);
        }

        [AllowAnonymous]
        [HttpPost("register")]
        public async Task<ActionResult<AdminResponseDto>> Register([FromBody] AdminCreateDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var admin = await _adminService.CrearAsync(dto);
                return CreatedAtAction(nameof(Register), new { id = admin.Id }, admin);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { mensaje = ex.Message });
            }
            catch
            {
                return StatusCode(500, new { mensaje = "Error interno al registrar el administrador." });
            }
        }

        [Authorize(Roles = "Admin")]
        [HttpPost("logout")]
        public async Task<IActionResult> Logout()
        {
            var adminId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(adminId))
                return Unauthorized();

            var token = Request.Headers["Authorization"].ToString().Replace("Bearer ", "");

            var tokenDb = await _context.TokenAdmins
                .FirstOrDefaultAsync(t => t.Token == token && t.AdminId == int.Parse(adminId));

            if (tokenDb == null) return NotFound("Token no encontrado.");

            tokenDb.EstaActivo = false;
            await _context.SaveChangesAsync();

            return Ok("Sesión cerrada exitosamente.");
        }

        // ──────────────────────────────────────────────────────────────
        // CRUD COMENTADO (Obtener, Eliminar)
        // ──────────────────────────────────────────────────────────────

        /*
        [Authorize(Roles = "Admin")]
        [HttpGet]
        public async Task<ActionResult<List<AdminResponseDto>>> ObtenerTodos()
        {
            var admins = await _adminService.ObtenerTodosAsync();
            return Ok(admins);
        }

        [Authorize(Roles = "Admin")]
        [HttpGet("{id}")]
        public async Task<ActionResult<AdminResponseDto>> ObtenerPorId(int id)
        {
            var admin = await _adminService.ObtenerPorIdAsync(id);
            if (admin == null) return NotFound();
            return Ok(admin);
        }

        [Authorize(Roles = "Admin")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> Eliminar(int id)
        {
            var eliminado = await _adminService.EliminarAsync(id);
            return eliminado ? NoContent() : NotFound();
        }
        */
    }
}
