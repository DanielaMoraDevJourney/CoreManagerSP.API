using CoreManagerSP.API.CoreManager.Application.DTOs.Historial;
using CoreManagerSP.API.CoreManager.Application.DTOs.Prestamo;
using CoreManagerSP.API.CoreManager.Application.DTOs.SimulacionDePrestamos;
using CoreManagerSP.API.CoreManager.Application.Interfaces.Prestamo;
using CoreManagerSP.API.CoreManager.Domain.Entities;
using CoreManagerSP.API.CoreManager.Infrastructure.Configurations;
using Microsoft.EntityFrameworkCore;

namespace CoreManagerSP.API.CoreManager.Application.Services.Prestamo
{
    public class SolicitudService : ISolicitudService
    {
        private readonly CoreManagerDbContext _context;

        public SolicitudService(CoreManagerDbContext context)
        {
            _context = context;
        }

        public async Task<SolicitudPrestamo?> ObtenerPorIdAsync(int id)
        {
            return await _context.Solicitudes
                .Include(s => s.AnalisisResultados)
                    .ThenInclude(r => r.EntidadFinanciera)
                .Include(s => s.TipoPrestamo)
                .Include(s => s.Usuario)
                .FirstOrDefaultAsync(s => s.Id == id);
        }

        public async Task<List<SolicitudPrestamo>> ObtenerPorUsuarioAsync(int usuarioId)
        {
            return await _context.Solicitudes
                .Where(s => s.UsuarioId == usuarioId)
                .Include(s => s.TipoPrestamo)
                .Include(s => s.AnalisisResultados)
                    .ThenInclude(r => r.EntidadFinanciera)
                .OrderByDescending(s => s.FechaSolicitud)
                .ToListAsync();
        }

        public async Task<SolicitudPrestamo> CrearAsync(SolicitudPrestamoCreateDto dto)
        {
            var solicitud = new SolicitudPrestamo
            {
                UsuarioId = dto.UsuarioId,
                TipoPrestamoId = dto.TipoPrestamoId,
                Monto = dto.Monto,
                Plazo = dto.Plazo,
                Proposito = dto.Proposito,
                CuotaEstimadaCliente = dto.CuotaEstimadaCliente,
                Estado = "Analizada",
                FechaSolicitud = DateTime.UtcNow
            };

            _context.Solicitudes.Add(solicitud);
            await _context.SaveChangesAsync();

            return solicitud;
        }

        public async Task<List<HistorialSimulacionDto>> ObtenerHistorialPorUsuarioAsync(int usuarioId)
        {
            var solicitudes = await _context.Solicitudes
                .Where(s => s.UsuarioId == usuarioId)
                .Include(s => s.TipoPrestamo)
                .Include(s => s.AnalisisResultados)
                    .ThenInclude(r => r.EntidadFinanciera)
                .OrderByDescending(s => s.FechaSolicitud)
                .ToListAsync();

            var historial = solicitudes.Select(s => new HistorialSimulacionDto
            {
                SolicitudId = s.Id,
                Fecha = s.FechaSolicitud,
                TipoPrestamo = s.TipoPrestamo.Nombre,
                Monto = s.Monto,
                Plazo = s.Plazo,
                Estado = s.Estado,
                Resultados = s.AnalisisResultados.Select(r => new HistorialResultadoEntidadDto
                {
                    NombreEntidad = r.EntidadFinanciera.Nombre,
                    Probabilidad = r.ProbabilidadAprobacion,
                    EsApto = r.EsApto
                }).ToList()
            }).ToList();

            return historial;
        }
    }
}
