using System.ComponentModel.DataAnnotations;

namespace CoreManagerSP.API.CoreManager.Domain.Entities
{
    public class AnalisisResultado
    {
        public int Id { get; set; }

        [Required]
        public int SolicitudPrestamoId { get; set; }

        [Required]
        public int EntidadFinancieraId { get; set; }

        [Range(0, 1)]
        public decimal ProbabilidadAprobacion { get; set; }

        [Range(0, double.MaxValue)]
        public decimal CuotaMensualEstimada { get; set; }

        public bool EsApto { get; set; }

        [MaxLength(300)]
        public string MensajeResumen { get; set; }

        public SolicitudPrestamo SolicitudPrestamo { get; set; }
        public EntidadFinanciera EntidadFinanciera { get; set; }

        public ICollection<MejoraSugerida> MejorasSugeridas { get; set; } = new List<MejoraSugerida>();
    }
}


