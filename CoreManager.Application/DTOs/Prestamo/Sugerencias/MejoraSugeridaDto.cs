namespace CoreManagerSP.API.CoreManager.Application.DTOs.Prestamo.Sugerencias
{
    namespace CoreManagerSP.API.CoreManager.Application.DTOs.Prestamo.Analisis
    {
        public class MejoraSugeridaDto
        {
            public string Variable { get; set; }
            public string ValorSugerido { get; set; }
            public string Descripcion { get; set; }
            public string ImpactoEstimado { get; set; }
            public bool EsObligatoria { get; set; }
            public int Prioridad { get; set; }
        }
    }

}
