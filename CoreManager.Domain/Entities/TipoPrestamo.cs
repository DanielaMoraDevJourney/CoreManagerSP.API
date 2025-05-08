using System.ComponentModel.DataAnnotations;

namespace CoreManagerSP.API.CoreManager.Domain.Entities
{
    public class TipoPrestamo
    {

        public int Id { get; set; }

        [Required]
        [MaxLength(100)]
        public string Nombre { get; set; }

        [MaxLength(300)]
        public string Descripcion { get; set; }

        [Required]
        public string TipoGeneral { get; set; }

        [Range(0.01, 100)]
        public decimal TasaBase { get; set; }

        [Range(0, double.MaxValue)]
        public decimal CuotaMinima { get; set; }

        public string OtrosRequisitos { get; set; }

        public ICollection<SolicitudPrestamo> Solicitudes { get; set; }
        public ICollection<EntidadTipoPrestamo> EntidadesTipoPrestamo { get; set; }

    }

}
