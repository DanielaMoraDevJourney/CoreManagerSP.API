namespace CoreManagerSP.API.CoreManager.Application.DTOs.EntidadesFinancieras
{
    public class EntidadFinancieraDto
    {
        public int Id { get; set; }
        public string Nombre { get; set; }
        public decimal TasaInteres { get; set; }
        public decimal IngresoMinimo { get; set; }
        public decimal RelacionCuotaIngresoMaxima { get; set; }
        public int AntiguedadHistorialMinima { get; set; }
        public bool AceptaMora { get; set; }
        public bool RequiereTarjetaCredito { get; set; }
        public List<string> TiposPrestamo { get; set; } 
    }

}
