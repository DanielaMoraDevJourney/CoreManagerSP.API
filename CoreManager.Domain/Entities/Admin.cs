namespace CoreManagerSP.API.CoreManager.Domain.Entities
{
    public class Admin
    {
        public int Id { get; set; }
        public string Nombre { get; set; }
        public string Correo { get; set; }
        public string ContrasenaHash { get; set; }

        public DateTime FechaRegistro { get; set; }

        public string Rol { get; set; } = "Admin";
    }


}
