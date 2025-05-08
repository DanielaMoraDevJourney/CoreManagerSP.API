using CoreManagerSP.API.CoreManager.Application.DTOs.EntidadesFinancieras;
using CoreManagerSP.API.CoreManager.Application.DTOs.Prestamo;
using CoreManagerSP.API.CoreManager.Application.DTOs.Prestamo.Analisis;
using CoreManagerSP.API.CoreManager.Application.DTOs.Prestamo.Historial;
using CoreManagerSP.API.CoreManager.Application.DTOs.Prestamo.Sugerencias;
using CoreManagerSP.API.CoreManager.Domain.Entities;

namespace CoreManagerSP.API.CoreManager.Application.Interfaces.Prestamo
{
    public interface ISolicitudPrestamoService
    {
        Task<SolicitudPrestamo> CrearAsync(SolicitudPrestamoCreateDto dto);
        Task<List<SolicitudPrestamo>> ObtenerPorUsuarioAsync(int usuarioId);
        Task<SolicitudPrestamo?> ObtenerPorIdAsync(int id);
        Task<List<AnalisisResultadoDto>> ObtenerRankingPorSolicitudAsync(int solicitudId);
        Task<AnalisisDetalleDto?> ObtenerAnalisisPorEntidadAsync(int solicitudId, int entidadId);
        Task<List<MejoraSugerida>> ObtenerMejorasPorEntidadAsync(int solicitudId, int entidadId);
        Task<bool> AplicarMejorasAsync(AplicarMejorasDto dto);
        Task<List<HistorialSimulacionDto>> ObtenerHistorialPorUsuarioAsync(int usuarioId);
        Task<bool> CrearSimulacionCompletaAsync(SimulacionCompletaDto dto);
        Task<AnalisisEntidadCompletoDto> ObtenerAnalisisCompletoPorEntidadAsync(int solicitudId, int entidadId);
        Task<List<AnalisisEntidadCompletoDto>> CompararEntidadesAsync(int solicitudId, List<int> entidadIds);



    }
}
