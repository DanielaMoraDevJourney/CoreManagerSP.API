using System.ComponentModel.DataAnnotations;

namespace CoreManagerSP.API.CoreManager.Domain.Entities
{
    public class LogSistema
    {

        public int Id { get; set; }

        [Required]
        public string Tipo { get; set; }

        [MaxLength(300)]
        public string Mensaje { get; set; }

        public DateTime Fecha { get; set; } = DateTime.UtcNow;

        public int? UsuarioId { get; set; }
        public Usuario Usuario { get; set; }
    }

}
