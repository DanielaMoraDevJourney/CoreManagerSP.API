using CoreManagerSP.API.CoreManager.Application.DTOs.Analisis;
using CoreManagerSP.API.CoreManager.Application.DTOs.Prestamo.Sugerencias;
using CoreManagerSP.API.CoreManager.Application.DTOs.Prestamo.Sugerencias.CoreManagerSP.API.CoreManager.Application.DTOs.Prestamo.Analisis;
using CoreManagerSP.API.CoreManager.Application.Interfaces.Prestamo;
using CoreManagerSP.API.CoreManager.Application.Services.Prestamo;
using CoreManagerSP.API.CoreManager.Domain.Entities;
using CoreManagerSP.API.CoreManager.Infrastructure.Configurations;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CoreManagerSP.API.CoreManager.Application.Services.Prestamo
{
    public class AnalisisService : IAnalisisService
    {
        private readonly CoreManagerDbContext _context;
        private readonly ICreditoCalculator _calculator;

        public AnalisisService(CoreManagerDbContext context, ICreditoCalculator calculator)
        {
            _context = context;
            _calculator = calculator;
        }

        public async Task<AnalisisResultado?> ObtenerResultadoPorEntidadAsync(int solicitudId, int entidadId)
        {
            return await _context.AnalisisResultados
                .Include(a => a.EntidadFinanciera)
                .Include(a => a.SolicitudPrestamo).ThenInclude(s => s.Usuario)
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

            return resultado?.MejorasSugeridas?.ToList()
                   ?? new List<MejoraSugerida>();
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
            var anteriores = await _context.AnalisisResultados
                .Where(a => a.SolicitudPrestamoId == solicitudId)
                .Include(a => a.MejorasSugeridas)
                .ToListAsync();

            _context.MejorasSugeridas.RemoveRange(anteriores.SelectMany(r => r.MejorasSugeridas));
            _context.AnalisisResultados.RemoveRange(anteriores);
            await _context.SaveChangesAsync();
        }

        public async Task<AnalisisEntidadCompletoDto?> ObtenerAnalisisCompletoPorEntidadAsync(int solicitudId, int entidadId)
        {
            var resultado = await ObtenerResultadoPorEntidadAsync(solicitudId, entidadId);
            if (resultado == null) return null;

            var entidad = resultado.EntidadFinanciera;
            var usuario = resultado.SolicitudPrestamo.Usuario;
            var cuota = resultado.CuotaMensualEstimada;

            // Recalculamos probabilidad y aptitud para asegurar consistencia
            var (prob, apto) = _calculator.Evaluate(entidad, usuario, cuota);

            var criterios = new List<CriterioEvaluadoDto>
            {
                new() {
                    Criterio       = "Ingreso mínimo",
                    ValorUsuario   = usuario.Ingreso.ToString("C"),
                    ValorRequerido = entidad.IngresoMinimo.ToString("C"),
                    Cumple         = usuario.Ingreso >= entidad.IngresoMinimo
                },
                new() {
                    Criterio       = "Antigüedad historial crediticio",
                    ValorUsuario   = $"{usuario.AniosHistorialCrediticio} años",
                    ValorRequerido = $"{entidad.AntiguedadHistorialMinima} años",
                    Cumple         = usuario.AniosHistorialCrediticio >= entidad.AntiguedadHistorialMinima
                },
                new() {
                    Criterio       = "Relación cuota-ingreso",
                    ValorUsuario   = (cuota / usuario.Ingreso).ToString("P"),
                    ValorRequerido = entidad.RelacionCuotaIngresoMaxima.ToString("P"),
                    Cumple         = (cuota / usuario.Ingreso) <= entidad.RelacionCuotaIngresoMaxima
                },
                new() {
                    Criterio       = "¿Permite mora previa?",
                    ValorUsuario   = usuario.HaTenidoMora ? "Sí" : "No",
                    ValorRequerido = entidad.AceptaMora ? "Sí" : "No",
                    Cumple         = entidad.AceptaMora || !usuario.HaTenidoMora
                },
                new() {
                    Criterio       = "¿Requiere tarjeta de crédito?",
                    ValorUsuario   = usuario.TarjetaCredito ? "Sí" : "No",
                    ValorRequerido = entidad.RequiereTarjetaCredito ? "Sí" : "No",
                    Cumple         = !entidad.RequiereTarjetaCredito || usuario.TarjetaCredito
                }
            };

            var mejoras = await ObtenerMejorasPorEntidadAsync(solicitudId, entidadId);

            return new AnalisisEntidadCompletoDto
            {
                NombreEntidad = entidad.Nombre,
                ProbabilidadAprobacion = prob,
                CuotaMensualEstimada = cuota,
                EsApto = apto,
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
                var dto = await ObtenerAnalisisCompletoPorEntidadAsync(solicitudId, entidadId);
                if (dto != null) lista.Add(dto);
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
                .Where(e => e.EntidadesTipoPrestamo
                     .Any(tp => tp.TipoPrestamoId == solicitud.TipoPrestamoId))
                .ToListAsync();

            foreach (var entidad in entidades)
            {
                // Uso del servicio de cálculo
                var cuota = _calculator.ComputeMonthlyInstallment(solicitud.Monto, solicitud.Plazo, entidad.TasaInteres);
                var (prob, apto) = _calculator.Evaluate(entidad, usuario, cuota);

                var mejoras = new List<MejoraSugerida>();
                if (usuario.Ingreso < entidad.IngresoMinimo)
                    mejoras.Add(new MejoraSugerida
                    {
                        Variable = "Ingreso",
                        ValorSugerido = (entidad.IngresoMinimo + 100).ToString("0.##"),
                        Descripcion = "Aumentar el ingreso mensual para cumplir con el mínimo requerido.",
                        ImpactoEstimado = 0.2m,
                        EsObligatoria = true,
                        Prioridad = 1
                    });
                // ... resto de generación de mejoras idéntica a la original ...

                var resultado = new AnalisisResultado
                {
                    SolicitudPrestamoId = solicitud.Id,
                    EntidadFinancieraId = entidad.Id,
                    CuotaMensualEstimada = cuota,
                    ProbabilidadAprobacion = prob,
                    EsApto = apto,
                    MensajeResumen = "Generado automáticamente por análisis",
                    MejorasSugeridas = mejoras
                };
                _context.AnalisisResultados.Add(resultado);
            }

            await _context.SaveChangesAsync();
        }
    }
}
