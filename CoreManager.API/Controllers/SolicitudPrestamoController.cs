using CoreManagerSP.API.CoreManager.Application.DTOs.EntidadesFinancieras;
using CoreManagerSP.API.CoreManager.Application.DTOs.Prestamo;
using CoreManagerSP.API.CoreManager.Application.DTOs.Prestamo.Analisis;
using CoreManagerSP.API.CoreManager.Application.DTOs.Prestamo.Historial;
using CoreManagerSP.API.CoreManager.Application.DTOs.Prestamo.Sugerencias;
using CoreManagerSP.API.CoreManager.Application.Interfaces.Prestamo;
using CoreManagerSP.API.CoreManager.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CoreManagerSP.API.CoreManager.API.Controllers
{
    [Authorize(Roles = "Usuario")]
    [ApiController]
    [Route("api/[controller]")]
    public class SolicitudPrestamoController : ControllerBase
    {
        private readonly ISolicitudPrestamoService _service;

        public SolicitudPrestamoController(ISolicitudPrestamoService service)
        {
            _service = service;
        }

        // ------------------------------------------------------------------------------------------
        // PRINCIPAL: Crear simulación completa
        // ------------------------------------------------------------------------------------------

        /// <summary>
        /// Crea una simulación completa (perfil + solicitud + análisis + mejoras).
        /// </summary>
        [HttpPost("simulacion-completa")]
        public async Task<IActionResult> CrearSimulacionCompleta([FromBody] SimulacionCompletaDto dto)
        {
            var success = await _service.CrearSimulacionCompletaAsync(dto);
            if (!success) return NotFound("Usuario no encontrado.");
            return Ok("Simulación registrada con perfil financiero.");
        }

        // ------------------------------------------------------------------------------------------
        // RESULTADOS INDIVIDUALES Y DETALLE POR ENTIDAD
        // ------------------------------------------------------------------------------------------

        /// <summary>
        /// Devuelve el análisis detallado completo (criterios + sugerencias) de una entidad.
        /// </summary>
        [HttpGet("{solicitudId}/analisis-entidad/{entidadId}")]
        public async Task<ActionResult<AnalisisEntidadCompletoDto>> ObtenerAnalisisEntidadCompleto(int solicitudId, int entidadId)
        {
            var resultado = await _service.ObtenerAnalisisCompletoPorEntidadAsync(solicitudId, entidadId);
            if (resultado == null) return NotFound("No se encontró el análisis para esta entidad.");
            return Ok(resultado);
        }

        /// <summary>
        /// Compara múltiples entidades mostrando criterios + mejoras de cada una.
        /// </summary>
        [HttpPost("comparar-entidades")]
        public async Task<ActionResult<List<AnalisisEntidadCompletoDto>>> CompararEntidades([FromBody] CompararEntidadesDto dto)
        {
            var comparativas = await _service.CompararEntidadesAsync(dto.SolicitudId, dto.EntidadesIds);
            return Ok(comparativas);
        }

        // ------------------------------------------------------------------------------------------
        // RESULTADOS GENERALES Y RANKING
        // ------------------------------------------------------------------------------------------

        /// <summary>
        /// Devuelve el ranking de entidades analizadas ordenado por probabilidad.
        /// </summary>
        [HttpGet("ranking/{solicitudId}")]
        public async Task<ActionResult<List<AnalisisResultadoDto>>> GetRanking(int solicitudId)
        {
            var ranking = await _service.ObtenerRankingPorSolicitudAsync(solicitudId);
            if (ranking == null || !ranking.Any()) return NotFound();
            return Ok(ranking);
        }

        // ------------------------------------------------------------------------------------------
        // HISTORIAL Y SEGUIMIENTO
        // ------------------------------------------------------------------------------------------

        /// <summary>
        /// Devuelve el historial completo de simulaciones hechas por el usuario.
        /// </summary>
        [HttpGet("historial/{usuarioId}")]
        public async Task<ActionResult<List<HistorialSimulacionDto>>> GetHistorial(int usuarioId)
        {
            var historial = await _service.ObtenerHistorialPorUsuarioAsync(usuarioId);
            return Ok(historial);
        }

        // ------------------------------------------------------------------------------------------
        // APLICACIÓN DE MEJORAS
        // ------------------------------------------------------------------------------------------

        /// <summary>
        /// Aplica mejoras al perfil del usuario y vuelve a ejecutar el análisis.
        /// </summary>
        [HttpPost("aplicar-mejoras")]
        public async Task<IActionResult> AplicarMejoras([FromBody] AplicarMejorasDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var exito = await _service.AplicarMejorasAsync(dto);
            if (!exito) return NotFound("No se encontró la solicitud o falló la aplicación de mejoras.");

            return Ok("Mejoras aplicadas y reanálisis realizado con éxito.");
        }

        // ------------------------------------------------------------------------------------------
        // OPCIONALES 
        // ------------------------------------------------------------------------------------------

        /// <summary>
        /// Devuelve todas las solicitudes hechas por un usuario.
        /// </summary>
        [HttpGet("usuario/{usuarioId}")]
        public async Task<ActionResult<List<SolicitudPrestamo>>> GetByUsuario(int usuarioId)
        {
            var solicitudes = await _service.ObtenerPorUsuarioAsync(usuarioId);
            return Ok(solicitudes);
        }

        /// <summary>
        /// Devuelve una solicitud por su ID.
        /// </summary>
        [HttpGet("{id}")]
        public async Task<ActionResult<SolicitudPrestamo>> GetById(int id)
        {
            var solicitud = await _service.ObtenerPorIdAsync(id);
            if (solicitud == null) return NotFound();
            return Ok(solicitud);
        }

        /// <summary>
        /// [OPCIONAL] Devuelve análisis por entidad (sin incluir mejoras).
        /// </summary>
        [HttpGet("{solicitudId}/entidad/{entidadId}")]
        public async Task<ActionResult<AnalisisDetalleDto>> GetDetallePorEntidad(int solicitudId, int entidadId)
        {
            var detalle = await _service.ObtenerAnalisisPorEntidadAsync(solicitudId, entidadId);
            if (detalle == null) return NotFound();
            return Ok(detalle);
        }

        /// <summary>
        /// [OPCIONAL] Devuelve todas las mejoras sugeridas para una solicitud y entidad.
        /// </summary>
        [HttpGet("{solicitudId}/entidad/{entidadId}/mejoras")]
        public async Task<ActionResult<List<MejoraSugerida>>> GetMejoras(int solicitudId, int entidadId)
        {
            var mejoras = await _service.ObtenerMejorasPorEntidadAsync(solicitudId, entidadId);
            return Ok(mejoras);
        }

        /// <summary>
        /// [OPCIONAL] Devuelve toda la información analizada y agrupada por entidad (uso interno).
        /// </summary>
        [HttpGet("resultado-completo/{solicitudId}")]
        public async Task<ActionResult<ResultadoCompletoDto>> GetResultadoCompleto(int solicitudId)
        {
            var solicitud = await _service.ObtenerPorIdAsync(solicitudId);
            if (solicitud == null) return NotFound();

            var entidadesAnalizadas = await _service.ObtenerRankingPorSolicitudAsync(solicitudId);
            var detalles = new List<DetalleResultadoEntidadDto>();

            foreach (var entidad in entidadesAnalizadas)
            {
                var detalle = await _service.ObtenerAnalisisPorEntidadAsync(solicitudId, entidad.EntidadId);
                var mejoras = await _service.ObtenerMejorasPorEntidadAsync(solicitudId, entidad.EntidadId);

                detalles.Add(new DetalleResultadoEntidadDto
                {
                    EntidadId = entidad.EntidadId,
                    NombreEntidad = entidad.NombreEntidad,
                    ProbabilidadAprobacion = entidad.ProbabilidadAprobacion,
                    CuotaMensualEstimada = entidad.CuotaMensualEstimada,
                    EsApto = entidad.EsApto,
                    Detalle = detalle,
                    Mejoras = mejoras
                });
            }

            var resultado = new ResultadoCompletoDto
            {
                SolicitudId = solicitud.Id,
                UsuarioId = solicitud.UsuarioId,
                Fecha = solicitud.FechaSolicitud,
                Monto = solicitud.Monto,
                Plazo = solicitud.Plazo,
                TipoPrestamo = solicitud.TipoPrestamo?.Nombre ?? "",
                Estado = solicitud.Estado,
                DetallesPorEntidad = detalles
            };

            return Ok(resultado);
        }
    }
}
