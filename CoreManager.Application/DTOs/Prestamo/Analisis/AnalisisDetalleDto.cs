namespace CoreManagerSP.API.CoreManager.Application.DTOs.Prestamo.Analisis
{
    public class AnalisisDetalleDto
    {
        public string NombreEntidad { get; set; }
        public decimal ProbabilidadAprobacion { get; set; }
        public decimal CuotaMensualEstimada { get; set; }
        public bool EsApto { get; set; }
        public List<CriterioEvaluadoDto> Criterios { get; set; }
    }
}
