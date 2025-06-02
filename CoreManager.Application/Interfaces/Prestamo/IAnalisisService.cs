using CoreManagerSP.API.CoreManager.Application.DTOs.Analisis;
using CoreManagerSP.API.CoreManager.Domain.Entities;

namespace CoreManagerSP.API.CoreManager.Application.Interfaces.Prestamo
{
    public interface IAnalisisService
    {
        Task<AnalisisResultado?> ObtenerResultadoPorEntidadAsync(int solicitudId, int entidadId);
        Task<List<MejoraSugerida>> ObtenerMejorasPorEntidadAsync(int solicitudId, int entidadId);
        Task<List<AnalisisResultado>> ObtenerRankingAsync(int solicitudId);
        Task<List<AnalisisResultado>> ObtenerTodosPorSolicitudAsync(int solicitudId);
        Task EliminarAnalisisYMejorasPreviosAsync(int solicitudId);
        Task<AnalisisEntidadCompletoDto?> ObtenerAnalisisCompletoPorEntidadAsync(int solicitudId, int entidadId);
        Task<List<AnalisisEntidadCompletoDto>> CompararEntidadesAsync(int solicitudId, List<int> entidadIds);
        Task<List<AnalisisResultadoDto>> ObtenerRankingPorSolicitudAsync(int solicitudId);
        Task AnalizarSolicitudAsync(int solicitudId);

    }
}
