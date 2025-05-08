namespace CoreManagerSP.API.CoreManager.Application.DTOs.Prestamo
{
    public class ResultadoCompletoDto
    {
        public int SolicitudId { get; set; }
        public int UsuarioId { get; set; }
        public DateTime Fecha { get; set; }
        public decimal Monto { get; set; }
        public int Plazo { get; set; }
        public string TipoPrestamo { get; set; }
        public string Estado { get; set; }
        public List<DetalleResultadoEntidadDto> DetallesPorEntidad { get; set; } = new();
    }
}
