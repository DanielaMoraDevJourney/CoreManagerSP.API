using System.ComponentModel.DataAnnotations;

namespace CoreManagerSP.API.CoreManager.Application.DTOs.EntidadFinanciera
{

    public class EntidadFinancieraCreateDto
    {
        [Required]
        public string Nombre { get; set; }

        [Range(0.01, 100)]
        public decimal TasaInteres { get; set; }

        [Range(0, double.MaxValue)]
        public decimal IngresoMinimo { get; set; }

        [Range(0.01, 1.0)]
        public decimal RelacionCuotaIngresoMaxima { get; set; }

        [Range(0, 100)]
        public int AntiguedadHistorialMinima { get; set; }

        public bool AceptaMora { get; set; }

        public bool RequiereTarjetaCredito { get; set; }

        public List<int> TiposPrestamoIds { get; set; }
    }
}
