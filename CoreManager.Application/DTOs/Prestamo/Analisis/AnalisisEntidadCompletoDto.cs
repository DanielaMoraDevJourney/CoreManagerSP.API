using CoreManagerSP.API.CoreManager.Application.DTOs.Prestamo.Sugerencias.CoreManagerSP.API.CoreManager.Application.DTOs.Prestamo.Analisis;

namespace CoreManagerSP.API.CoreManager.Application.DTOs.Prestamo.Analisis
{
    public class AnalisisEntidadCompletoDto
    {
        public string NombreEntidad { get; set; }
        public decimal ProbabilidadAprobacion { get; set; }
        public decimal CuotaMensualEstimada { get; set; }
        public bool EsApto { get; set; }
        public List<CriterioEvaluadoDto> Criterios { get; set; }
        public List<MejoraSugeridaDto> Mejoras { get; set; }
    }


}
