namespace CoreManagerSP.API.CoreManager.Application.DTOs.Logeo
{
    public class AdminLoginResponseDto
    {
        public int Id { get; set; }
        public string Nombre { get; set; }
        public string Correo { get; set; }
        public string Rol { get; set; }
        public string Token { get; set; }
    }

}
