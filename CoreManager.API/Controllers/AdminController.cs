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

        // ───────────────────────────────────────────────────────────────────────────────
        // AUTENTICACIÓN Y GESTIÓN DE SESIONES
        // ───────────────────────────────────────────────────────────────────────────────

        /// <summary>
        /// Inicia sesión como administrador.
        /// </summary>
        /// <param name="dto">Credenciales de acceso (correo y contraseña).</param>
        /// <returns>Token JWT y datos del administrador.</returns>
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

        /// <summary>
        /// Cierra sesión y desactiva el token activo.
        /// </summary>
        /// <returns>Mensaje de éxito si se cerró sesión correctamente.</returns>
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

        // ───────────────────────────────────────────────────────────────────────────────
        // CRUD DE ADMINISTRADORES
        // ───────────────────────────────────────────────────────────────────────────────

        /// <summary>
        /// Crea un nuevo administrador.
        /// </summary>
        /// <param name="dto">Datos del nuevo administrador.</param>
        /// <returns>Administrador creado.</returns>
        [AllowAnonymous]
        [HttpPost("crear")]
        public async Task<ActionResult<AdminResponseDto>> Crear([FromBody] AdminCreateDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var admin = await _adminService.CrearAsync(dto);
                return CreatedAtAction(nameof(ObtenerPorId), new { id = admin.Id }, admin);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { mensaje = ex.Message });
            }
            catch
            {
                return StatusCode(500, new { mensaje = "Error interno al crear el administrador." });
            }
        }

        /// <summary>
        /// Obtiene la lista de todos los administradores registrados.
        /// </summary>
        [Authorize(Roles = "Admin")]
        [HttpGet]
        public async Task<ActionResult<List<AdminResponseDto>>> ObtenerTodos()
        {
            var admins = await _adminService.ObtenerTodosAsync();
            return Ok(admins);
        }

        /// <summary>
        /// Obtiene un administrador específico por su ID.
        /// </summary>
        /// <param name="id">ID del administrador.</param>
        /// <returns>Datos del administrador si existe.</returns>
        [Authorize(Roles = "Admin")]
        [HttpGet("{id}")]
        public async Task<ActionResult<AdminResponseDto>> ObtenerPorId(int id)
        {
            var admin = await _adminService.ObtenerPorIdAsync(id);
            if (admin == null) return NotFound();
            return Ok(admin);
        }

        /// <summary>
        /// Elimina un administrador por su ID.
        /// </summary>
        /// <param name="id">ID del administrador a eliminar.</param>
        /// <returns>NoContent si se eliminó, NotFound si no existe.</returns>
        [Authorize(Roles = "Admin")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> Eliminar(int id)
        {
            var eliminado = await _adminService.EliminarAsync(id);
            return eliminado ? NoContent() : NotFound();
        }
    }
}
