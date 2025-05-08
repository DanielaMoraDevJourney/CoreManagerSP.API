namespace CoreManagerSP.API.CoreManager.Domain.Entities
{
    public class TokenAdmin
    {
        public int Id { get; set; }
        public int AdminId { get; set; }
        public string Token { get; set; }
        public DateTime FechaExpiracion { get; set; }
        public bool EstaActivo { get; set; }

        public Admin Admin { get; set; }
    }

}
