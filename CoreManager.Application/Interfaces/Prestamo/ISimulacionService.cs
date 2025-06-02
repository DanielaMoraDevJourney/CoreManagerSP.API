using CoreManagerSP.API.CoreManager.Application.DTOs.SimulacionDePrestamos;
using CoreManagerSP.API.CoreManager.Domain.Entities;

namespace CoreManagerSP.API.CoreManager.Application.Interfaces.Prestamo
{
    public interface ISimulacionService
    {
        decimal CalcularCuota(decimal monto, int plazoMeses, decimal tasaInteresAnual);
        decimal CalcularProbabilidad(EntidadFinanciera entidad, Usuario usuario, SolicitudPrestamoCreateDto dto);
        bool EsAprobado(EntidadFinanciera entidad, Usuario usuario, SolicitudPrestamoCreateDto dto);
        Task<List<EntidadFinanciera>> ObtenerEntidadesPorTipoPrestamoAsync(int tipoPrestamoId);
        Task<bool> CrearSimulacionCompletaAsync(SimulacionCompletaDto dto);

    }
}
