namespace CoreManagerSP.API.CoreManager.Application.DTOs.Logeo
{
    public class AdminResponseDto
    {
        public int Id { get; set; }
        public string Nombre { get; set; }
        public string Correo { get; set; }
        public DateTime FechaRegistro { get; set; }
        public string Rol { get; set; }
    }
}
