namespace CoreManagerSP.API.CoreManager.Application.DTOs.Historial
{
    public class HistorialSimulacionDto
    {
        public int SolicitudId { get; set; }
        public DateTime Fecha { get; set; }
        public string TipoPrestamo { get; set; }
        public decimal Monto { get; set; }
        public int Plazo { get; set; }
        public string Estado { get; set; }
        public List<HistorialResultadoEntidadDto> Resultados { get; set; }
    }
}
