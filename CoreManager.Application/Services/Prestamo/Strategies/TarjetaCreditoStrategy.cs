using CoreManagerSP.API.CoreManager.Domain.Entities;

namespace CoreManagerSP.API.CoreManager.Application.Services.Prestamo.Strategies
{

    public class TarjetaCreditoStrategy : IUsuarioMejoraStrategy
    {
        public bool CanHandle(string variable) =>
            string.Equals(variable, "TarjetaCredito", StringComparison.OrdinalIgnoreCase);

        public void Apply(Usuario usuario, string nuevoValor, List<HistorialMejoras> cambios, int solicitudId)
        {
            var valorAnterior = usuario.TarjetaCredito.ToString();
            if (bool.TryParse(nuevoValor, out var tarjetaNueva))
            {
                usuario.TarjetaCredito = tarjetaNueva;
                cambios.Add(new HistorialMejoras
                {
                    SolicitudPrestamoId = solicitudId,
                    VariableModificada = "TarjetaCredito",
                    ValorAnterior = valorAnterior,
                    ValorNuevo = nuevoValor,
                    FechaAplicacion = DateTime.UtcNow
                });
            }
        }
    }
}
