using AutoMapper;
using CoreManagerSP.API.CoreManager.API.Filters;
using CoreManagerSP.API.CoreManager.Application.DTOs.EntidadesFinancieras;
using CoreManagerSP.API.CoreManager.Application.DTOs.EntidadFinanciera;
using CoreManagerSP.API.CoreManager.Application.Interfaces.EntidadesFinancieras;
using CoreManagerSP.API.CoreManager.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CoreManagerSP.API.CoreManager.API.Controllers
{
    [Authorize(Roles = "Admin")]
    [ApiController]
    [Route("api/[controller]")]
    public class EntidadFinancieraController : ControllerBase
    {
        private readonly IEntidadFinancieraService _service;
        private readonly IMapper _mapper;

        public EntidadFinancieraController(
            IEntidadFinancieraService service,
            IMapper mapper)
        {
            _service = service;
            _mapper = mapper;
        }

        /// <summary>
        /// Obtiene todas las entidades financieras registradas.
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<List<EntidadFinancieraDto>>> Get()
        {
            var entidades = await _service.ObtenerTodosAsync();
            var dtoList = _mapper.Map<List<EntidadFinancieraDto>>(entidades);
            return Ok(dtoList);
        }

        /// <summary>
        /// Obtiene una entidad financiera por su ID.
        /// </summary>
        [HttpGet("{id}")]
        public async Task<ActionResult<EntidadFinancieraDto>> GetById(int id)
        {
            var entidad = await _service.ObtenerPorIdAsync(id);
            if (entidad == null)
                return NotFound(new { mensaje = "Entidad financiera no encontrada." });

            var dto = _mapper.Map<EntidadFinancieraDto>(entidad);
            return Ok(dto);
        }

        /// <summary>
        /// Crea una nueva entidad financiera.
        /// </summary>
        [HttpPost]
        public async Task<ActionResult<EntidadFinancieraDto>> Create(
            [FromBody] EntidadFinancieraCreateDto dto)
        {
            // La validación de ModelState la maneja ValidationFilterAttribute
            var entidad = await _service.CrearAsync(dto);
            var result = _mapper.Map<EntidadFinancieraDto>(entidad);
            return CreatedAtAction(nameof(GetById), new { id = entidad.Id }, result);
        }

        /// <summary>
        /// Elimina una entidad financiera por su ID.
        /// </summary>
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var eliminado = await _service.EliminarAsync(id);
            return eliminado
                ? NoContent()
                : NotFound(new { mensaje = "Entidad financiera no encontrada para eliminar." });
        }
    }
}
