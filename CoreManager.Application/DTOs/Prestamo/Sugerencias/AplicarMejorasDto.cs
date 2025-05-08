namespace CoreManagerSP.API.CoreManager.Application.DTOs.Prestamo.Sugerencias
{
    public class AplicarMejorasDto
    {
        public int SolicitudId { get; set; }
        public Dictionary<string, string> MejorasAplicadas { get; set; }
    }

}
