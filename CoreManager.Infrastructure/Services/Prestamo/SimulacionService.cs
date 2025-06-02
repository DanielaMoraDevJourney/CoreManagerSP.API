using CoreManagerSP.API.CoreManager.Application.DTOs.SimulacionDePrestamos;
using CoreManagerSP.API.CoreManager.Application.Interfaces.Prestamo;
using CoreManagerSP.API.CoreManager.Domain.Entities;
using CoreManagerSP.API.CoreManager.Infrastructure.Configurations;
using Microsoft.EntityFrameworkCore;

namespace CoreManagerSP.API.CoreManager.Application.Services.Prestamo
{
    public class SimulacionService : ISimulacionService
    {
        private readonly CoreManagerDbContext _context;
        private readonly IAnalisisService _analisisService;

        public SimulacionService(CoreManagerDbContext context, IAnalisisService analisisService)
        {
            _context = context;
            _analisisService = analisisService;
        }

        public decimal CalcularCuota(decimal monto, int plazoMeses, decimal tasaInteresAnual)
        {
            if (plazoMeses <= 0 || tasaInteresAnual <= 0) return 0;

            var tasaMensual = tasaInteresAnual / 12 / 100;
            return monto * tasaMensual / (1 - (decimal)Math.Pow(1 + (double)tasaMensual, -plazoMeses));
        }

        public decimal CalcularProbabilidad(EntidadFinanciera entidad, Usuario usuario, SolicitudPrestamoCreateDto dto)
        {
            decimal score = 1;

            if (usuario.Ingreso < entidad.IngresoMinimo)
                score -= 0.25m;

            if (usuario.AniosHistorialCrediticio < entidad.AntiguedadHistorialMinima)
                score -= 0.20m;

            if (usuario.HaTenidoMora && !entidad.AceptaMora)
                score -= 0.20m;

            if (entidad.RequiereTarjetaCredito && !usuario.TarjetaCredito)
                score -= 0.10m;

            var cuotaCalculada = CalcularCuota(dto.Monto, dto.Plazo, entidad.TasaInteres);

            if (usuario.Ingreso <= 0 || cuotaCalculada / usuario.Ingreso > entidad.RelacionCuotaIngresoMaxima)
                score -= 0.25m;

            return Math.Clamp(score, 0, 1);
        }

        public bool EsAprobado(EntidadFinanciera entidad, Usuario usuario, SolicitudPrestamoCreateDto dto)
        {
            var cuota = CalcularCuota(dto.Monto, dto.Plazo, entidad.TasaInteres);

            return usuario.Ingreso >= entidad.IngresoMinimo &&
                   usuario.AniosHistorialCrediticio >= entidad.AntiguedadHistorialMinima &&
                   (entidad.AceptaMora || !usuario.HaTenidoMora) &&
                   (!entidad.RequiereTarjetaCredito || usuario.TarjetaCredito) &&
                   usuario.Ingreso > 0 && (cuota / usuario.Ingreso) <= entidad.RelacionCuotaIngresoMaxima;
        }

        public async Task<List<EntidadFinanciera>> ObtenerEntidadesPorTipoPrestamoAsync(int tipoPrestamoId)
        {
            return await _context.EntidadesFinancieras
                .Include(e => e.EntidadesTipoPrestamo)
                .Where(e => e.EntidadesTipoPrestamo.Any(t => t.TipoPrestamoId == tipoPrestamoId))
                .ToListAsync();
        }

        public async Task<bool> CrearSimulacionCompletaAsync(SimulacionCompletaDto dto)
        {
            var usuario = await _context.Usuarios.FindAsync(dto.UsuarioId);
            if (usuario == null) return false;

            // Actualizar perfil financiero
            usuario.Ingreso = dto.Ingreso;
            usuario.TarjetaCredito = dto.TarjetaCredito;
            usuario.HaTenidoMora = dto.HaTenidoMora;
            usuario.AniosHistorialCrediticio = dto.AniosHistorialCrediticio;
            usuario.DeudasVigentes = dto.DeudasVigentes;
            usuario.CuotasMensualesComprometidas = dto.CuotasMensualesComprometidas;
            usuario.NumeroCreditosActivos = dto.NumeroCreditosActivos;
            usuario.TiempoUltimoIncumplimiento = dto.TiempoUltimoIncumplimiento;

            _context.Usuarios.Update(usuario);
            await _context.SaveChangesAsync();

            var solicitud = new SolicitudPrestamo
            {
                UsuarioId = dto.UsuarioId,
                TipoPrestamoId = dto.TipoPrestamoId,
                Monto = dto.Monto,
                Plazo = dto.Plazo,
                Proposito = dto.Proposito,
                CuotaEstimadaCliente = dto.CuotaEstimadaCliente,
                Estado = "Analizada",
                FechaSolicitud = DateTime.UtcNow
            };

            _context.Solicitudes.Add(solicitud);
            await _context.SaveChangesAsync();

            // ✔ Ejecutar análisis completo incluyendo mejoras
            await _analisisService.AnalizarSolicitudAsync(solicitud.Id);

            return true;
        }

    }
}
