namespace CoreManagerSP.API.CoreManager.Domain.Entities
{
    public class TokenUsuario
    {
        public int Id { get; set; }
        public int UsuarioId { get; set; }
        public string Token { get; set; } = string.Empty;
        public DateTime FechaExpiracion { get; set; }
        public bool EstaActivo { get; set; } = true;

        public Usuario Usuario { get; set; } = null!;
    }

}
