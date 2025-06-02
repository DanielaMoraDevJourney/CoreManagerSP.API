using CoreManagerSP.API.CoreManager.Application.DTOs.Analisis;

namespace CoreManagerSP.API.CoreManager.Application.DTOs.Mejoras
{
    public class SolicitudResumenDto
    {
        public int Id { get; set; }
        public DateTime Fecha { get; set; }
        public string Estado { get; set; }
        public decimal Monto { get; set; }
        public int Plazo { get; set; }
        public decimal CuotaEstimada { get; set; }
        public string TipoPrestamo { get; set; }
        public List<AnalisisResultadoDto> Ranking { get; set; }
    }
}
