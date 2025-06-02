using CoreManagerSP.API.CoreManager.Domain.Entities;
using CoreManagerSP.API.CoreManager.Application.DTOs.Mejoras;


namespace CoreManagerSP.API.CoreManager.Application.Interfaces.Prestamo
{
    public interface IMejorasService
    {
        Task<List<MejoraSugerida>> ObtenerMejorasPorEntidadAsync(int solicitudId, int entidadId);
        Task<bool> AplicarMejorasAsync(AplicarMejorasDto dto);
        Task<ResultadoMejorasAplicadasDto> AplicarMejorasAvanzadoAsync(AplicarMejorasSimuladasDto dto);
        Task<ResultadoMejorasAplicadasDto> AplicarMejorasAutoAsync(int solicitudId);

    }
}
