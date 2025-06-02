using CoreManagerSP.API.CoreManager.Application.DTOs.Analisis;
using CoreManagerSP.API.CoreManager.Application.DTOs.Prestamo.Sugerencias;
using CoreManagerSP.API.CoreManager.Application.Interfaces.Prestamo;
using CoreManagerSP.API.CoreManager.Domain.Entities;
using CoreManagerSP.API.CoreManager.Infrastructure.Configurations;
using Microsoft.EntityFrameworkCore;
using CoreManagerSP.API.CoreManager.Application.DTOs.Prestamo.Sugerencias.CoreManagerSP.API.CoreManager.Application.DTOs.Prestamo.Analisis;

namespace CoreManagerSP.API.CoreManager.Application.Services.Prestamo
{
    public class AnalisisService : IAnalisisService
    {
        private readonly CoreManagerDbContext _context;

        public AnalisisService(CoreManagerDbContext context)
        {
            _context = context;
        }

        public async Task<AnalisisResultado?> ObtenerResultadoPorEntidadAsync(int solicitudId, int entidadId)
        {
            return await _context.AnalisisResultados
                .Include(a => a.EntidadFinanciera)
                .Include(a => a.SolicitudPrestamo)
                    .ThenInclude(s => s.Usuario)
                .FirstOrDefaultAsync(a =>
                    a.SolicitudPrestamoId == solicitudId &&
                    a.EntidadFinancieraId == entidadId);
        }

        public async Task<List<MejoraSugerida>> ObtenerMejorasPorEntidadAsync(int solicitudId, int entidadId)
        {
            var resultado = await _context.AnalisisResultados
                .Include(a => a.MejorasSugeridas)
                .FirstOrDefaultAsync(a =>
                    a.SolicitudPrestamoId == solicitudId &&
                    a.EntidadFinancieraId == entidadId);

            return resultado?.MejorasSugeridas?.ToList() ?? new List<MejoraSugerida>();
        }

        public async Task<List<AnalisisResultado>> ObtenerRankingAsync(int solicitudId)
        {
            return await _context.AnalisisResultados
                .Include(a => a.EntidadFinanciera)
                .Where(a => a.SolicitudPrestamoId == solicitudId)
                .OrderByDescending(a => a.ProbabilidadAprobacion)
                .ToListAsync();
        }

        public async Task<List<AnalisisResultado>> ObtenerTodosPorSolicitudAsync(int solicitudId)
        {
            return await _context.AnalisisResultados
                .Include(a => a.EntidadFinanciera)
                .Where(a => a.SolicitudPrestamoId == solicitudId)
                .ToListAsync();
        }

        public async Task EliminarAnalisisYMejorasPreviosAsync(int solicitudId)
        {
            var resultadosAnteriores = await _context.AnalisisResultados
                .Where(a => a.SolicitudPrestamoId == solicitudId)
                .Include(a => a.MejorasSugeridas)
                .ToListAsync();

            _context.MejorasSugeridas.RemoveRange(resultadosAnteriores.SelectMany(r => r.MejorasSugeridas));
            _context.AnalisisResultados.RemoveRange(resultadosAnteriores);
            await _context.SaveChangesAsync();
        }

        public async Task<AnalisisEntidadCompletoDto?> ObtenerAnalisisCompletoPorEntidadAsync(int solicitudId, int entidadId)
        {
            var resultado = await ObtenerResultadoPorEntidadAsync(solicitudId, entidadId);
            if (resultado == null) return null;

            var entidad = resultado.EntidadFinanciera;
            var usuario = resultado.SolicitudPrestamo.Usuario;
            var cuota = resultado.CuotaMensualEstimada;

            var criterios = new List<CriterioEvaluadoDto>
            {
                new() {
                    Criterio = "Ingreso mínimo",
                    ValorUsuario = usuario.Ingreso.ToString("C"),
                    ValorRequerido = entidad.IngresoMinimo.ToString("C"),
                    Cumple = usuario.Ingreso >= entidad.IngresoMinimo
                },
                new() {
                    Criterio = "Antigüedad historial crediticio",
                    ValorUsuario = $"{usuario.AniosHistorialCrediticio} años",
                    ValorRequerido = $"{entidad.AntiguedadHistorialMinima} años",
                    Cumple = usuario.AniosHistorialCrediticio >= entidad.AntiguedadHistorialMinima
                },
                new() {
                    Criterio = "Relación cuota-ingreso",
                    ValorUsuario = (cuota / usuario.Ingreso).ToString("P"),
                    ValorRequerido = entidad.RelacionCuotaIngresoMaxima.ToString("P"),
                    Cumple = (cuota / usuario.Ingreso) <= entidad.RelacionCuotaIngresoMaxima
                },
                new() {
                    Criterio = "¿Permite mora previa?",
                    ValorUsuario = usuario.HaTenidoMora ? "Sí" : "No",
                    ValorRequerido = entidad.AceptaMora ? "Sí" : "No",
                    Cumple = entidad.AceptaMora || !usuario.HaTenidoMora
                },
                new() {
                    Criterio = "¿Requiere tarjeta de crédito?",
                    ValorUsuario = usuario.TarjetaCredito ? "Sí" : "No",
                    ValorRequerido = entidad.RequiereTarjetaCredito ? "Sí" : "No",
                    Cumple = !entidad.RequiereTarjetaCredito || usuario.TarjetaCredito
                }
            };

            var mejoras = await ObtenerMejorasPorEntidadAsync(solicitudId, entidadId);

            return new AnalisisEntidadCompletoDto
            {
                NombreEntidad = entidad.Nombre,
                ProbabilidadAprobacion = resultado.ProbabilidadAprobacion,
                CuotaMensualEstimada = cuota,
                EsApto = resultado.EsApto,
                Criterios = criterios,
                Mejoras = mejoras.Select(m => new MejoraSugeridaDto
                {
                    Variable = m.Variable,
                    ValorSugerido = m.ValorSugerido,
                    Descripcion = m.Descripcion,
                    ImpactoEstimado = m.ImpactoEstimado,
                    EsObligatoria = m.EsObligatoria,
                    Prioridad = m.Prioridad
                }).ToList()
            };
        }

        public async Task<List<AnalisisEntidadCompletoDto>> CompararEntidadesAsync(int solicitudId, List<int> entidadIds)
        {
            var lista = new List<AnalisisEntidadCompletoDto>();
            foreach (var entidadId in entidadIds)
            {
                var item = await ObtenerAnalisisCompletoPorEntidadAsync(solicitudId, entidadId);
                if (item != null) lista.Add(item);
            }
            return lista;
        }

        public async Task<List<AnalisisResultadoDto>> ObtenerRankingPorSolicitudAsync(int solicitudId)
        {
            var resultados = await ObtenerRankingAsync(solicitudId);
            return resultados.Select(r => new AnalisisResultadoDto
            {
                EntidadId = r.EntidadFinancieraId,
                NombreEntidad = r.EntidadFinanciera.Nombre,
                ProbabilidadAprobacion = r.ProbabilidadAprobacion,
                CuotaMensualEstimada = r.CuotaMensualEstimada,
                EsApto = r.EsApto
            }).ToList();
        }

        public async Task AnalizarSolicitudAsync(int solicitudId)
        {
            var solicitud = await _context.Solicitudes
                .Include(s => s.Usuario)
                .Include(s => s.TipoPrestamo)
                .FirstOrDefaultAsync(s => s.Id == solicitudId);

            if (solicitud == null) throw new Exception("Solicitud no encontrada.");

            var usuario = solicitud.Usuario;
            var entidades = await _context.EntidadesFinancieras
                .Include(e => e.EntidadesTipoPrestamo)
                .Where(e => e.EntidadesTipoPrestamo.Any(tp => tp.TipoPrestamoId == solicitud.TipoPrestamoId))
                .ToListAsync();

            foreach (var entidad in entidades)
            {
                var cuota = CalcularCuota(solicitud.Monto, solicitud.Plazo, entidad.TasaInteres);
                var probabilidad = CalcularProbabilidad(entidad, usuario, solicitud);
                var esApto = EsAprobado(entidad, usuario, solicitud);

                var mejoras = new List<MejoraSugerida>();

                if (usuario.Ingreso < entidad.IngresoMinimo)
                {
                    mejoras.Add(new MejoraSugerida
                    {
                        Variable = "Ingreso",
                        ValorSugerido = (entidad.IngresoMinimo + 100).ToString("0.##"),
                        Descripcion = "Aumentar el ingreso mensual para cumplir con el mínimo requerido.",
                        ImpactoEstimado = 0.2m,
                        EsObligatoria = true,
                        Prioridad = 1
                    });
                }

                if (usuario.AniosHistorialCrediticio < entidad.AntiguedadHistorialMinima)
                {
                    mejoras.Add(new MejoraSugerida
                    {
                        Variable = "AniosHistorialCrediticio",
                        ValorSugerido = entidad.AntiguedadHistorialMinima.ToString(),
                        Descripcion = "Esperar hasta cumplir con la antigüedad crediticia requerida.",
                        ImpactoEstimado = 0.1m,
                        EsObligatoria = false,
                        Prioridad = 2
                    });
                }

                if (usuario.Ingreso > 0 && solicitud.CuotaEstimadaCliente / usuario.Ingreso > entidad.RelacionCuotaIngresoMaxima)
                {
                    var nuevoLimite = entidad.RelacionCuotaIngresoMaxima * usuario.Ingreso;
                    mejoras.Add(new MejoraSugerida
                    {
                        Variable = "CuotasMensualesComprometidas",
                        ValorSugerido = nuevoLimite.ToString("0.##"),
                        Descripcion = "Reducir cuotas mensuales comprometidas para mejorar relación cuota-ingreso.",
                        ImpactoEstimado = 0.15m,
                        EsObligatoria = true,
                        Prioridad = 1
                    });
                }

                if (usuario.HaTenidoMora && !entidad.AceptaMora)
                {
                    mejoras.Add(new MejoraSugerida
                    {
                        Variable = "HaTenidoMora",
                        ValorSugerido = "false",
                        Descripcion = "Mantener un historial sin moras durante un tiempo para mejorar el perfil crediticio.",
                        ImpactoEstimado = 0.2m,
                        EsObligatoria = true,
                        Prioridad = 2
                    });
                }

                if (!usuario.TarjetaCredito && entidad.RequiereTarjetaCredito)
                {
                    mejoras.Add(new MejoraSugerida
                    {
                        Variable = "TarjetaCredito",
                        ValorSugerido = "true",
                        Descripcion = "Obtener una tarjeta de crédito activa para cumplir con los requisitos de la entidad.",
                        ImpactoEstimado = 0.1m,
                        EsObligatoria = false,
                        Prioridad = 3
                    });
                }

                var resultado = new AnalisisResultado
                {
                    SolicitudPrestamoId = solicitud.Id,
                    EntidadFinancieraId = entidad.Id,
                    CuotaMensualEstimada = cuota,
                    ProbabilidadAprobacion = probabilidad,
                    EsApto = esApto,
                    MensajeResumen = "Generado automáticamente por análisis",
                    MejorasSugeridas = mejoras
                };

                _context.AnalisisResultados.Add(resultado);
            }

            await _context.SaveChangesAsync();
        }

        private decimal CalcularCuota(decimal monto, int plazoMeses, decimal tasaInteresAnual)
        {
            if (plazoMeses <= 0 || tasaInteresAnual <= 0) return 0;
            var tasaMensual = tasaInteresAnual / 12 / 100;
            return monto * tasaMensual / (1 - (decimal)Math.Pow(1 + (double)tasaMensual, -plazoMeses));
        }

        private decimal CalcularProbabilidad(EntidadFinanciera entidad, Usuario usuario, SolicitudPrestamo solicitud)
        {
            decimal score = 1;

            if (usuario.Ingreso < entidad.IngresoMinimo) score -= 0.25m;
            if (usuario.AniosHistorialCrediticio < entidad.AntiguedadHistorialMinima) score -= 0.20m;
            if (usuario.HaTenidoMora && !entidad.AceptaMora) score -= 0.20m;
            if (entidad.RequiereTarjetaCredito && !usuario.TarjetaCredito) score -= 0.10m;

            var cuota = CalcularCuota(solicitud.Monto, solicitud.Plazo, entidad.TasaInteres);
            if (usuario.Ingreso <= 0 || cuota / usuario.Ingreso > entidad.RelacionCuotaIngresoMaxima) score -= 0.25m;

            return Math.Clamp(score, 0, 1);
        }

        private bool EsAprobado(EntidadFinanciera entidad, Usuario usuario, SolicitudPrestamo solicitud)
        {
            var cuota = CalcularCuota(solicitud.Monto, solicitud.Plazo, entidad.TasaInteres);

            return usuario.Ingreso >= entidad.IngresoMinimo &&
                   usuario.AniosHistorialCrediticio >= entidad.AntiguedadHistorialMinima &&
                   (entidad.AceptaMora || !usuario.HaTenidoMora) &&
                   (!entidad.RequiereTarjetaCredito || usuario.TarjetaCredito) &&
                   usuario.Ingreso > 0 && (cuota / usuario.Ingreso) <= entidad.RelacionCuotaIngresoMaxima;
        }
    }
}
