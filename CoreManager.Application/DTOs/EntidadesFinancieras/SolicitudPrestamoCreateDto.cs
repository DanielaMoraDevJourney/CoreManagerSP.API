using System.ComponentModel.DataAnnotations;

namespace CoreManagerSP.API.CoreManager.Application.DTOs.EntidadesFinancieras
{
    public class SolicitudPrestamoCreateDto
    {
        [Required]
        public int UsuarioId { get; set; }

        [Required]
        public int TipoPrestamoId { get; set; }

        [Range(1000, 1000000)]
        public decimal Monto { get; set; }

        [Range(6, 360)]
        public int Plazo { get; set; }

        [Required]
        public string Proposito { get; set; }

        [Range(0, double.MaxValue)]
        public decimal CuotaEstimadaCliente { get; set; }
    }
}
