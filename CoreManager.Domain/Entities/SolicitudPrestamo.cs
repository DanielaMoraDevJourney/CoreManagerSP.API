using System.ComponentModel.DataAnnotations;

namespace CoreManagerSP.API.CoreManager.Domain.Entities
{
    public class SolicitudPrestamo
    {
        public int Id { get; set; }

        [Required]
        public int UsuarioId { get; set; }

        [Required]
        public int TipoPrestamoId { get; set; }

        [Range(1000, 1000000)]
        public decimal Monto { get; set; }

        [Range(6, 360)]
        public int Plazo { get; set; }

        [Required]
        [MaxLength(100)]
        public string Proposito { get; set; }

        [Range(0, double.MaxValue)]
        public decimal CuotaEstimadaCliente { get; set; }

        public string Estado { get; set; } = "Creada";

        public DateTime FechaSolicitud { get; set; } = DateTime.UtcNow;

        public Usuario Usuario { get; set; }
        public TipoPrestamo TipoPrestamo { get; set; }

        public ICollection<AnalisisResultado> AnalisisResultados { get; set; }
    }

}
