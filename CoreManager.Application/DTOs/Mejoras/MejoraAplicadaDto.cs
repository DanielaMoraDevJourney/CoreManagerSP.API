namespace CoreManagerSP.API.CoreManager.Application.DTOs.Mejoras
{
    public class MejoraAplicadaDto
    {
        public string Variable { get; set; }
        public string ValorNuevo { get; set; }
        public bool EsManual { get; set; } = false;
    }
}
