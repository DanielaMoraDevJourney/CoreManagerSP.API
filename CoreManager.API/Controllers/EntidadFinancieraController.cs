using CoreManagerSP.API.CoreManager.Application.DTOs.EntidadFinanciera;
using CoreManagerSP.API.CoreManager.Application.Interfaces.EntidadesFinancieras;
using CoreManagerSP.API.CoreManager.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CoreManagerSP.API.CoreManager.API.Controllers
{
    [Authorize(Roles = "Admin")]
    [ApiController]
    [Route("api/[controller]")]
    public class EntidadFinancieraController : ControllerBase
    {
        private readonly IEntidadFinancieraService _service;

        public EntidadFinancieraController(IEntidadFinancieraService service)
        {
            _service = service;
        }

        /// <summary>
        /// Obtiene todas las entidades financieras registradas.
        /// </summary>
        /// <returns>Lista de entidades financieras.</returns>
        [HttpGet]
        public async Task<ActionResult<List<EntidadFinanciera>>> Get()
        {
            var entidades = await _service.ObtenerTodosAsync();
            return Ok(entidades);
        }

        /// <summary>
        /// Obtiene una entidad financiera por su ID.
        /// </summary>
        /// <param name="id">ID de la entidad financiera.</param>
        /// <returns>Entidad financiera encontrada o mensaje de error.</returns>
        [HttpGet("{id}")]
        public async Task<ActionResult<EntidadFinanciera>> GetById(int id)
        {
            var entidad = await _service.ObtenerPorIdAsync(id);
            if (entidad == null)
                return NotFound(new { mensaje = "Entidad financiera no encontrada." });

            return Ok(entidad);
        }

        /// <summary>
        /// Crea una nueva entidad financiera.
        /// </summary>
        /// <param name="dto">DTO con los datos de la entidad financiera.</param>
        /// <returns>Entidad financiera creada o error en la creación.</returns>
        [HttpPost]
        public async Task<ActionResult<EntidadFinanciera>> Create([FromBody] EntidadFinancieraCreateDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var entidad = await _service.CrearAsync(dto);
                return CreatedAtAction(nameof(GetById), new { id = entidad.Id }, entidad);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { mensaje = "Error al crear la entidad financiera.", detalle = ex.Message });
            }
        }

        /// <summary>
        /// Elimina una entidad financiera por su ID.
        /// </summary>
        /// <param name="id">ID de la entidad financiera a eliminar.</param>
        /// <returns>NoContent si fue eliminada o NotFound si no existe.</returns>
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var eliminado = await _service.EliminarAsync(id);
                if (!eliminado)
                    return NotFound(new { mensaje = "Entidad financiera no encontrada para eliminar." });

                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { mensaje = "Error al eliminar la entidad financiera.", detalle = ex.Message });
            }
        }
    }
}
