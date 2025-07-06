using CoreManagerSP.API.CoreManager.Domain.Entities;

namespace CoreManagerSP.API.CoreManager.Application.Services.Prestamo.Strategies
{

    public interface IUsuarioMejoraStrategy
    {
        /// <summary>
        /// ¿Esta estrategia maneja la variable indicada?
        /// </summary>
        bool CanHandle(string variable);

        /// <summary>
        /// Aplica el cambio en el usuario y registra el historial.
        /// </summary>
        void Apply(Usuario usuario, string nuevoValor, List<HistorialMejoras> cambios, int solicitudId);
    }
}
