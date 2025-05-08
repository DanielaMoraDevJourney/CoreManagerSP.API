using System.ComponentModel.DataAnnotations;

namespace CoreManagerSP.API.CoreManager.Domain.Entities
{
    public class HistorialMejoras
    {
        public int Id { get; set; }

        [Required]
        public int SolicitudPrestamoId { get; set; }

        [MaxLength(100)]
        public string VariableModificada { get; set; }

        public string ValorAnterior { get; set; }
        public string ValorNuevo { get; set; }

        public DateTime FechaAplicacion { get; set; }

        public SolicitudPrestamo SolicitudPrestamo { get; set; }
    }

}
