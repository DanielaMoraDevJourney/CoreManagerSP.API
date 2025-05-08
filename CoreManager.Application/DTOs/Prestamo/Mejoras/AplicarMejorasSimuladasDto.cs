namespace CoreManagerSP.API.CoreManager.Application.DTOs.Prestamo.Mejoras
{
    public class AplicarMejorasSimuladasDto
    {
        public int SolicitudId { get; set; }

        public List<MejoraAplicadaDto> MejorasSeleccionadas { get; set; }
    }
}
