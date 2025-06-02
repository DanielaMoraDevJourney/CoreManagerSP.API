namespace CoreManagerSP.API.CoreManager.Application.DTOs.Mejoras
{
    public class AplicarMejoraItemDto
    {
        public string Variable { get; set; } = string.Empty;
        public string ValorNuevo { get; set; } = string.Empty;
        public bool EsManual { get; set; } = false;
    }
}
