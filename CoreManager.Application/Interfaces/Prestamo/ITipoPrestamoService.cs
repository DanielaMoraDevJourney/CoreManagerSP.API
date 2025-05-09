using CoreManagerSP.API.CoreManager.Application.DTOs.Prestamo.TipoPrestamo;
using CoreManagerSP.API.CoreManager.Domain.Entities;

namespace CoreManagerSP.API.CoreManager.Application.Interfaces.Prestamo
{
    public interface ITipoPrestamoService
    {
        Task<List<TipoPrestamo>> ObtenerTodosAsync();
        Task<TipoPrestamo?> ObtenerPorIdAsync(int id);
        Task<TipoPrestamo> CrearAsync(TipoPrestamoCreateDto dto);
        Task<bool> EliminarAsync(int id);
    }
}
