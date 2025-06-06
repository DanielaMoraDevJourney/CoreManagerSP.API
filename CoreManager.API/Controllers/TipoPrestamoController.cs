﻿using CoreManagerSP.API.CoreManager.Application.DTOs.TipoPrestamo;
using CoreManagerSP.API.CoreManager.Application.Interfaces.Prestamo;
using CoreManagerSP.API.CoreManager.Domain.Entities;
using CoreManagerSP.API.CoreManager.Infrastructure.Configurations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CoreManagerSP.API.CoreManager.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TipoPrestamoController : ControllerBase
    {
        private readonly ITipoPrestamoService _service;
        private readonly CoreManagerDbContext _context;


        public TipoPrestamoController(ITipoPrestamoService service, CoreManagerDbContext context)
        {
            _service = service;
            _context = context;
        }

        /// <summary>
        /// Obtiene todos los tipos de préstamo disponibles en el sistema.
        /// </summary>
        /// <returns>Lista de tipos de préstamo.</returns>
        
        [Authorize(Roles = "Usuario,Admin")]
        [HttpGet]
        public async Task<ActionResult<List<TipoPrestamo>>> Get()
        {
            var tipos = await _service.ObtenerTodosAsync();
            return Ok(tipos);
        }

        /// <summary>
        /// Obtiene un tipo de préstamo por su ID.
        /// </summary>
        /// <param name="id">ID del tipo de préstamo.</param>
        /// <returns>Tipo de préstamo encontrado o mensaje de error.</returns>
        /// 
        [Authorize(Roles = "Admin")]
        [HttpGet("{id}")]
        public async Task<ActionResult<TipoPrestamo>> GetById(int id)
        {
            var tipo = await _service.ObtenerPorIdAsync(id);
            if (tipo == null)
                return NotFound("Tipo de préstamo no encontrado.");
            return Ok(tipo);
        }

        /// <summary>
        /// Crea un nuevo tipo de préstamo.
        /// </summary>
        /// <param name="dto">DTO con los datos del tipo de préstamo a crear.</param>
        /// <returns>Tipo de préstamo creado o mensaje de error.</returns>

        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<ActionResult<TipoPrestamo>> Create([FromBody] TipoPrestamoCreateDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var tipo = await _service.CrearAsync(dto);
                return CreatedAtAction(nameof(GetById), new { id = tipo.Id }, tipo);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(ex.Message); // Ya existe
            }
            catch (Exception)
            {
                return StatusCode(500, "Error interno al crear el tipo de préstamo.");
            }
        }

        /// <summary>
        /// Elimina un tipo de préstamo por su ID.
        /// </summary>
        /// <param name="id">ID del tipo de préstamo a eliminar.</param>
        /// <returns>NoContent si fue eliminado correctamente, o NotFound si no existe.</returns>
        [Authorize(Roles = "Admin")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var eliminado = await _service.EliminarAsync(id);
            return eliminado ? NoContent() : NotFound("No se encontró el tipo de préstamo para eliminar.");
        }






    }
}
