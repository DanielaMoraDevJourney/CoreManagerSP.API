using System.ComponentModel.DataAnnotations;

namespace CoreManagerSP.API.CoreManager.Application.DTOs.TipoPrestamo
{
    public class TipoPrestamoCreateDto
    {
        [Required]
        public string Nombre { get; set; }

        public string Descripcion { get; set; }

        [Required]
        public string TipoGeneral { get; set; }

        [Range(0.01, 100)]
        public decimal TasaBase { get; set; }

        [Range(0, double.MaxValue)]
        public decimal CuotaMinima { get; set; }

        public string OtrosRequisitos { get; set; }
    }
}
