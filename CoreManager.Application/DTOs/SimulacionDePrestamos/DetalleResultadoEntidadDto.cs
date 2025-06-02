using CoreManagerSP.API.CoreManager.Application.DTOs.Analisis;
using CoreManagerSP.API.CoreManager.Domain.Entities;

namespace CoreManagerSP.API.CoreManager.Application.DTOs.SimulacionDePrestamos
{
    public class DetalleResultadoEntidadDto
    {
        public int EntidadId { get; set; }
        public string NombreEntidad { get; set; }
        public decimal ProbabilidadAprobacion { get; set; }
        public decimal CuotaMensualEstimada { get; set; }
        public bool EsApto { get; set; }
        public AnalisisDetalleDto Detalle { get; set; }
        public List<MejoraSugerida> Mejoras { get; set; } = new();
    }
}
