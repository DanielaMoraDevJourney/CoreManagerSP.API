namespace CoreManagerSP.API.CoreManager.Application.DTOs.SimulacionDePrestamos
{
    public class SimulacionCompletaDto
    {
        public int UsuarioId { get; set; }
        public decimal Ingreso { get; set; }
        public bool TarjetaCredito { get; set; }
        public bool HaTenidoMora { get; set; }
        public int AniosHistorialCrediticio { get; set; }
        public decimal DeudasVigentes { get; set; }
        public decimal CuotasMensualesComprometidas { get; set; }
        public int NumeroCreditosActivos { get; set; }
        public string TiempoUltimoIncumplimiento { get; set; }

        public int TipoPrestamoId { get; set; }
        public decimal Monto { get; set; }
        public int Plazo { get; set; }
        public string Proposito { get; set; }
        public decimal CuotaEstimadaCliente { get; set; }
    }

}
