using CoreManagerSP.API.CoreManager.Application.DTOs.EntidadFinanciera;
using CoreManagerSP.API.CoreManager.Domain.Entities;

namespace CoreManagerSP.API.CoreManager.Application.Interfaces.EntidadesFinancieras
{
    public interface IEntidadFinancieraService
    {
        Task<List<EntidadFinanciera>> ObtenerTodosAsync();
        Task<EntidadFinanciera?> ObtenerPorIdAsync(int id);
        Task<EntidadFinanciera> CrearAsync(EntidadFinancieraCreateDto dto);
        Task<bool> EliminarAsync(int id);
    }
}
