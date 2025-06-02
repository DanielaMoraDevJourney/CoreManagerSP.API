namespace CoreManagerSP.API.CoreManager.Application.DTOs.Mejoras
{
    public class AplicarMejorasSimuladasDto
    {
        public int SolicitudId { get; set; }

        public List<MejoraAplicadaDto> MejorasSeleccionadas { get; set; }
    }
}
