using System.ComponentModel.DataAnnotations;

namespace CoreManagerSP.API.CoreManager.Application.DTOs.Usuarios
{
    public class UsuarioCreateDto
    {
        [Required]
        [MaxLength(50)]
        public string Nombre { get; set; }

        [Required]
        [MaxLength(50)]
        public string Apellido { get; set; }

        [Required]
        [EmailAddress]
        public string Correo { get; set; }

        [Required]
        [MinLength(8)]
        public string Contrasena { get; set; }

        [Range(0, double.MaxValue)]
        public decimal Ingreso { get; set; }

        [Required]
        public string NivelHistorialCrediticio { get; set; }

        [Range(0, double.MaxValue)]
        public decimal DeudasVigentes { get; set; }

        [Range(0, double.MaxValue)]
        public decimal CuotasMensualesComprometidas { get; set; }

        [Range(0, int.MaxValue)]
        public int NumeroCreditosActivos { get; set; }

        public bool HaTenidoMora { get; set; }

        public string TiempoUltimoIncumplimiento { get; set; }

        public bool TarjetaCredito { get; set; }

        [Range(0, 100)]
        public int AniosHistorialCrediticio { get; set; }
    }
}
