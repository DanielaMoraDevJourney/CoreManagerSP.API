using CoreManagerSP.API.CoreManager.Application.DTOs.Historial;
using CoreManagerSP.API.CoreManager.Application.DTOs.Prestamo;
using CoreManagerSP.API.CoreManager.Application.DTOs.SimulacionDePrestamos;
using CoreManagerSP.API.CoreManager.Domain.Entities;

namespace CoreManagerSP.API.CoreManager.Application.Interfaces.Prestamo
{
    public interface ISolicitudService
    {
        Task<SolicitudPrestamo?> ObtenerPorIdAsync(int id);
        Task<List<SolicitudPrestamo>> ObtenerPorUsuarioAsync(int usuarioId);
        Task<SolicitudPrestamo> CrearAsync(SolicitudPrestamoCreateDto dto);
        Task<List<HistorialSimulacionDto>> ObtenerHistorialPorUsuarioAsync(int usuarioId);
    }
}