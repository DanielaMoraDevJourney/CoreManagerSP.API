using CoreManagerSP.API.CoreManager.Domain.Entities;

namespace CoreManagerSP.API.CoreManager.Application.Services.Prestamo.Strategies
{

    public class IngresoStrategy : IUsuarioMejoraStrategy
    {
        public bool CanHandle(string variable) =>
            string.Equals(variable, "Ingreso", StringComparison.OrdinalIgnoreCase);

        public void Apply(Usuario usuario, string nuevoValor, List<HistorialMejoras> cambios, int solicitudId)
        {
            var valorAnterior = usuario.Ingreso.ToString();
            if (decimal.TryParse(nuevoValor, out var ingresoNuevo))
            {
                usuario.Ingreso = ingresoNuevo;
                cambios.Add(new HistorialMejoras
                {
                    SolicitudPrestamoId = solicitudId,
                    VariableModificada = "Ingreso",
                    ValorAnterior = valorAnterior,
                    ValorNuevo = nuevoValor,
                    FechaAplicacion = DateTime.UtcNow
                });
            }
        }
    }
}

