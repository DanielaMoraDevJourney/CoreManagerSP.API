namespace CoreManagerSP.API.CoreManager.Domain.Entities
{
    public class EntidadTipoPrestamo
    {
        public int EntidadFinancieraId { get; set; }
        public EntidadFinanciera EntidadFinanciera { get; set; }

        public int TipoPrestamoId { get; set; }
        public TipoPrestamo TipoPrestamo { get; set; }
    }

}
