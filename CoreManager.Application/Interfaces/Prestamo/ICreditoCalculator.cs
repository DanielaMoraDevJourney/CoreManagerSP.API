using CoreManagerSP.API.CoreManager.Domain.Entities;

namespace CoreManagerSP.API.CoreManager.Application.Interfaces.Prestamo
{

    /// <summary>
    /// Servicio que encapsula los cálculos financieros relacionados con un préstamo.
    /// </summary>
    public interface ICreditoCalculator
    {
        /// <summary>
        /// Calcula la cuota mensual de un préstamo dado el monto, plazo en meses y tasa anual.
        /// </summary>
        decimal ComputeMonthlyInstallment(decimal monto, int plazoMeses, decimal tasaAnual);

        /// <summary>
        /// Evalúa la probabilidad de aprobación y determina si está aprobado,
        /// basándose en los criterios de la entidad y el usuario, y la cuota mensual calculada.
        /// </summary>
        (decimal probability, bool approved) Evaluate(EntidadFinanciera entidad, Usuario usuario, decimal cuotaMensual);
    }
}
