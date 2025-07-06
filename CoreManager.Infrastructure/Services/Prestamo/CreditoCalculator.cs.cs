using CoreManagerSP.API.CoreManager.Application.Interfaces.Prestamo;
using CoreManagerSP.API.CoreManager.Domain.Entities;

namespace CoreManagerSP.API.CoreManager.Infrastructure.Services.Prestamo
{

    public class CreditoCalculator : ICreditoCalculator
    {
        public decimal ComputeMonthlyInstallment(decimal monto, int plazoMeses, decimal tasaAnual)
        {
            if (plazoMeses <= 0 || tasaAnual <= 0)
                return 0;

            var tasaMensual = tasaAnual / 12m / 100m;
            return monto * tasaMensual
                 / (1 - (decimal)Math.Pow(1 + (double)tasaMensual, -plazoMeses));
        }

        public (decimal probability, bool approved) Evaluate(EntidadFinanciera e, Usuario u, decimal cuotaMensual)
        {
            decimal score = 1m;

            if (u.Ingreso < e.IngresoMinimo) score -= 0.25m;
            if (u.AniosHistorialCrediticio < e.AntiguedadHistorialMinima) score -= 0.20m;
            if (u.HaTenidoMora && !e.AceptaMora) score -= 0.20m;
            if (e.RequiereTarjetaCredito && !u.TarjetaCredito) score -= 0.10m;
            if (u.Ingreso <= 0 || cuotaMensual / u.Ingreso > e.RelacionCuotaIngresoMaxima)
                score -= 0.25m;

            // Clamp entre 0 y 1
            var probability = Math.Clamp(score, 0m, 1m);

            var approved =
                u.Ingreso >= e.IngresoMinimo &&
                u.AniosHistorialCrediticio >= e.AntiguedadHistorialMinima &&
                (e.AceptaMora || !u.HaTenidoMora) &&
                (!e.RequiereTarjetaCredito || u.TarjetaCredito) &&
                u.Ingreso > 0 &&
                (cuotaMensual / u.Ingreso) <= e.RelacionCuotaIngresoMaxima;

            return (probability, approved);
        }
    }
}
