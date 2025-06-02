using System.ComponentModel.DataAnnotations;

namespace CoreManagerSP.API.CoreManager.Domain.Entities
{
    public class MejoraSugerida
    {

        public int Id { get; set; }

        [Required]
        public int AnalisisResultadoId { get; set; }

        [Required]
        [MaxLength(50)]
        public string Variable { get; set; }

        [MaxLength(100)]
        public string ValorSugerido { get; set; }

        [MaxLength(300)]
        public string Descripcion { get; set; }

        public decimal ImpactoEstimado { get; set; }
        public bool EsObligatoria { get; set; }

        [Range(1, 5)]
        public int Prioridad { get; set; }

        public AnalisisResultado AnalisisResultado { get; set; }
    }

}
