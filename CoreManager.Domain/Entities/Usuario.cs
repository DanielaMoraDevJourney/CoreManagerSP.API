namespace CoreManagerSP.API.CoreManager.Domain.Entities
{
    using System.ComponentModel.DataAnnotations;

    public class Usuario
    {
        public int Id { get; set; }

        [Required]
        [MaxLength(50)]
        public string Nombre { get; set; } = string.Empty;

        [Required]
        [MaxLength(50)]
        public string Apellido { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        public string Correo { get; set; } = string.Empty;

        [Required]
        [MinLength(8)]
        public string Contrasena { get; set; } = string.Empty;

        [Range(0, double.MaxValue, ErrorMessage = "El ingreso debe ser mayor o igual a 0")]
        public decimal Ingreso { get; set; }

        public string NivelHistorialCrediticio { get; set; } = string.Empty;

        [Range(0, double.MaxValue)]
        public decimal DeudasVigentes { get; set; }

        [Range(0, double.MaxValue)]
        public decimal CuotasMensualesComprometidas { get; set; }

        [Range(0, int.MaxValue)]
        public int NumeroCreditosActivos { get; set; }

        public bool HaTenidoMora { get; set; }

        public string TiempoUltimoIncumplimiento { get; set; } = string.Empty;

        public bool TarjetaCredito { get; set; }

        [Range(0, 100)]
        public int AniosHistorialCrediticio { get; set; }

        public DateTime FechaRegistro { get; set; } = DateTime.UtcNow;

        public ICollection<SolicitudPrestamo> Solicitudes { get; set; } = new List<SolicitudPrestamo>();
    }


}
