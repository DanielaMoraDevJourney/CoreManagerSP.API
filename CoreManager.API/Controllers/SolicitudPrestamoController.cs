using CoreManagerSP.API.CoreManager.Application.DTOs.Analisis;
using CoreManagerSP.API.CoreManager.Application.DTOs.Mejoras;
using CoreManagerSP.API.CoreManager.Application.DTOs.Prestamo.Sugerencias;
using CoreManagerSP.API.CoreManager.Application.DTOs.SimulacionDePrestamos;
using CoreManagerSP.API.CoreManager.Application.DTOs.TipoPrestamo;
using CoreManagerSP.API.CoreManager.Application.Interfaces.Prestamo;
using CoreManagerSP.API.CoreManager.Infrastructure.Configurations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CoreManagerSP.API.CoreManager.API.Controllers
{
    [Authorize(Roles = "Usuario")]
    [ApiController]
    [Route("api/[controller]")]
    public class SolicitudPrestamoController : ControllerBase
    {
        private readonly ISolicitudService _solicitudService;
        private readonly ISimulacionService _simulacionService;
        private readonly IAnalisisService _analisisService;
        private readonly IMejorasService _mejorasService;
        private readonly CoreManagerDbContext _context;


        public SolicitudPrestamoController(
            ISolicitudService solicitudService,
            ISimulacionService simulacionService,
            IAnalisisService analisisService,
            IMejorasService mejorasService,
            CoreManagerDbContext context)
        {
            _solicitudService = solicitudService;
            _simulacionService = simulacionService;
            _analisisService = analisisService;
            _mejorasService = mejorasService;
            _context = context;
        }

        // ───────────────────────────────────────────────────────────────────────────────
        // ENDPOINTS ACTIVOS
        // ───────────────────────────────────────────────────────────────────────────────

        [HttpPost("simulacion-completa")]
        public async Task<IActionResult> CrearSimulacionCompleta([FromBody] SimulacionCompletaDto dto)
        {
            var success = await _simulacionService.CrearSimulacionCompletaAsync(dto);
            if (!success) return NotFound("Usuario no encontrado.");

            var solicitud = await _solicitudService.ObtenerPorUsuarioAsync(dto.UsuarioId);
            var nuevaSolicitudId = solicitud
                .OrderByDescending(s => s.FechaSolicitud)
                .ThenByDescending(s => s.Id)
                .FirstOrDefault()?.Id;


            return Ok(new { mensaje = "Simulación registrada.", solicitudId = nuevaSolicitudId });
        }


        [HttpGet("{solicitudId}/analisis-entidad/{entidadId}")]
        public async Task<ActionResult<AnalisisEntidadCompletoDto>> ObtenerAnalisisEntidadCompleto(int solicitudId, int entidadId)
        {
            var resultado = await _analisisService.ObtenerAnalisisCompletoPorEntidadAsync(solicitudId, entidadId);
            if (resultado == null) return NotFound("No se encontró el análisis para esta entidad.");
            return Ok(resultado);
        }

        [HttpPost("comparar-entidades")]
        public async Task<ActionResult<List<AnalisisEntidadCompletoDto>>> CompararEntidades([FromBody] CompararEntidadesDto dto)
        {
            var comparativas = await _analisisService.CompararEntidadesAsync(dto.SolicitudId, dto.EntidadesIds);
            return Ok(comparativas);
        }

        [HttpGet("ranking/{solicitudId}")]
        public async Task<ActionResult<List<AnalisisResultadoDto>>> GetRanking(int solicitudId)
        {
            var ranking = await _analisisService.ObtenerRankingPorSolicitudAsync(solicitudId);
            if (ranking == null || !ranking.Any()) return NotFound();
            return Ok(ranking);
        }

       

        [HttpPost("aplicar-mejoras-avanzado")]
        public async Task<ActionResult<ResultadoMejorasAplicadasDto>> AplicarMejorasAvanzado([FromBody] AplicarMejorasSimuladasDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var resultado = await _mejorasService.AplicarMejorasAvanzadoAsync(dto);
                return Ok(resultado);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    mensaje = "Ocurrió un error al aplicar mejoras avanzadas.",
                    detalle = ex.Message
                });
            }
        }

        [HttpGet("combo")]
        public async Task<IActionResult> ObtenerParaCombo()
        {
            var tipos = await _context.TiposPrestamo
                .Select(tp => new TipoPrestamoComboDto
                {
                    Id = tp.Id,
                    Nombre = tp.Nombre
                })
                .ToListAsync();

            return Ok(tipos);
        }
    }
}
