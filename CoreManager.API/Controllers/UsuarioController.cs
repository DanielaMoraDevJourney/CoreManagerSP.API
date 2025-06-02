using CoreManagerSP.API.CoreManager.Application.DTOs.Usuarios;
using CoreManagerSP.API.CoreManager.Application.Interfaces.Usuarios;
using CoreManagerSP.API.CoreManager.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[Authorize(Roles = "Admin")]
[ApiController]
[Route("api/[controller]")]
public class UsuarioController : ControllerBase
{
    private readonly IUsuarioService _usuarioService;

    public UsuarioController(IUsuarioService usuarioService)
    {
        _usuarioService = usuarioService;
    }

    /// <summary>
    /// [ADMIN] Lista todos los usuarios registrados en el sistema.
    /// </summary>
    [HttpGet("admin/listar")]
    public async Task<ActionResult<List<Usuario>>> ListarUsuariosAdmin()
    {
        var usuarios = await _usuarioService.ObtenerTodosAsync();
        return Ok(usuarios);
    }

    /// <summary>
    /// [ADMIN] Obtiene un usuario por su ID.
    /// </summary>
    [HttpGet("admin/{id}")]
    public async Task<ActionResult<Usuario>> ObtenerUsuarioPorIdAdmin(int id)
    {
        var usuario = await _usuarioService.ObtenerPorIdAsync(id);
        if (usuario == null) return NotFound();
        return Ok(usuario);
    }

    /// <summary>
    /// [ADMIN] Registra un nuevo usuario desde el panel de administración.
    /// </summary>
    [HttpPost("admin/registrar")]
    public async Task<ActionResult<Usuario>> RegistrarUsuarioDesdeAdmin([FromBody] UsuarioCreateDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        try
        {
            var usuario = await _usuarioService.CrearAsync(dto);
            return CreatedAtAction(nameof(ObtenerUsuarioPorIdAdmin), new { id = usuario.Id }, usuario);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { mensaje = ex.Message });
        }
        catch (Exception)
        {
            return StatusCode(500, new { mensaje = "Ocurrió un error inesperado al crear el usuario." });
        }
    }

    /// <summary>
    /// [ADMIN] Elimina un usuario por su ID.
    /// </summary>
    [HttpDelete("admin/eliminar/{id}")]
    public async Task<IActionResult> EliminarUsuarioAdmin(int id)
    {
        var eliminado = await _usuarioService.EliminarAsync(id);
        if (!eliminado) return NotFound();
        return NoContent();
    }
}
