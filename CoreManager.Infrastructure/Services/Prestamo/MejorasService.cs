using CoreManagerSP.API.CoreManager.Application.DTOs.Mejoras;
using CoreManagerSP.API.CoreManager.Application.DTOs.Prestamo.Sugerencias;
using CoreManagerSP.API.CoreManager.Application.DTOs.Prestamo;
using CoreManagerSP.API.CoreManager.Application.Interfaces.Prestamo;
using CoreManagerSP.API.CoreManager.Application.Services.Prestamo.Strategies;
using CoreManagerSP.API.CoreManager.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using CoreManagerSP.API.CoreManager.Application.DTOs.Analisis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CoreManagerSP.API.CoreManager.Infrastructure.Configurations;

namespace CoreManagerSP.API.CoreManager.Application.Services.Prestamo
{
    public class MejorasService : IMejorasService
    {
        private readonly CoreManagerDbContext _context;
        private readonly IAnalisisService _analisisService;
        private readonly IEnumerable<IUsuarioMejoraStrategy> _strategies;

        public MejorasService(
            CoreManagerDbContext context,
            IAnalisisService analisisService,
            IEnumerable<IUsuarioMejoraStrategy> strategies)
        {
            _context = context;
            _analisisService = analisisService;
            _strategies = strategies;
        }

        public async Task<List<MejoraSugerida>> ObtenerMejorasPorEntidadAsync(int solicitudId, int entidadId)
        {
            return await _analisisService.ObtenerMejorasPorEntidadAsync(solicitudId, entidadId);
        }

        public async Task<bool> AplicarMejorasAsync(AplicarMejorasDto dto)
        {
            var solicitud = await _context.Solicitudes
                .Include(s => s.Usuario)
                .FirstOrDefaultAsync(s => s.Id == dto.SolicitudId);

            if (solicitud == null) return false;

            var usuario = solicitud.Usuario;
            var cambios = new List<HistorialMejoras>();

            foreach (var mejora in dto.MejorasAplicadas)
            {
                var variable = mejora.Key;
                var nuevoValor = mejora.Value;

                var strategy = _strategies.FirstOrDefault(s => s.CanHandle(variable));
                if (strategy != null)
                {
                    strategy.Apply(usuario, nuevoValor, cambios, solicitud.Id);
                }
            }

            _context.HistorialMejoras.AddRange(cambios);
            _context.Usuarios.Update(usuario);
            await _analisisService.EliminarAnalisisYMejorasPreviosAsync(solicitud.Id);
            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<ResultadoMejorasAplicadasDto> AplicarMejorasAvanzadoAsync(AplicarMejorasSimuladasDto dto)
        {
            var solicitudOriginal = await _context.Solicitudes
                .Include(s => s.Usuario)
                .Include(s => s.TipoPrestamo)
                .FirstOrDefaultAsync(s => s.Id == dto.SolicitudId);

            if (solicitudOriginal == null)
                throw new Exception("Solicitud no encontrada.");

            var usuario = solicitudOriginal.Usuario;
            var cambiosAplicados = new List<CambioVariableDto>();

            foreach (var mejora in dto.MejorasSeleccionadas)
            {
                var variable = mejora.Variable;
                var nuevoValor = mejora.ValorNuevo;

                var strategy = _strategies.FirstOrDefault(s => s.CanHandle(variable));
                if (strategy != null)
                {
                    // Reutilizamos la lógica de estrategia para generar historial
                    var registro = new List<HistorialMejoras>();
                    strategy.Apply(usuario, nuevoValor, registro, solicitudOriginal.Id);

                    foreach (var hist in registro)
                    {
                        cambiosAplicados.Add(new CambioVariableDto
                        {
                            Variable = hist.VariableModificada,
                            ValorAnterior = hist.ValorAnterior,
                            ValorNuevo = hist.ValorNuevo
                        });
                    }
                }
            }

            _context.Usuarios.Update(usuario);
            await _context.SaveChangesAsync();

            var nuevaSolicitud = new SolicitudPrestamo
            {
                UsuarioId = usuario.Id,
                TipoPrestamoId = solicitudOriginal.TipoPrestamoId,
                Monto = solicitudOriginal.Monto,
                Plazo = solicitudOriginal.Plazo,
                Proposito = solicitudOriginal.Proposito + " (mejorado)",
                CuotaEstimadaCliente = solicitudOriginal.CuotaEstimadaCliente,
                Estado = "Analizada",
                FechaSolicitud = DateTime.UtcNow
            };

            _context.Solicitudes.Add(nuevaSolicitud);
            await _context.SaveChangesAsync();

            await _analisisService.EliminarAnalisisYMejorasPreviosAsync(nuevaSolicitud.Id);
            await _analisisService.AnalizarSolicitudAsync(nuevaSolicitud.Id);

            var rankingOriginal = await _analisisService.ObtenerRankingPorSolicitudAsync(solicitudOriginal.Id);
            var rankingNuevo = await _analisisService.ObtenerRankingPorSolicitudAsync(nuevaSolicitud.Id);

            return new ResultadoMejorasAplicadasDto
            {
                Mensaje = "Mejoras aplicadas y nueva simulación generada.",
                Fecha = nuevaSolicitud.FechaSolicitud,
                CambiosAplicados = cambiosAplicados,
                SolicitudOriginal = new SolicitudResumenDto
                {
                    Id = solicitudOriginal.Id,
                    Fecha = solicitudOriginal.FechaSolicitud,
                    Estado = solicitudOriginal.Estado,
                    Monto = solicitudOriginal.Monto,
                    Plazo = solicitudOriginal.Plazo,
                    CuotaEstimada = solicitudOriginal.CuotaEstimadaCliente,
                    TipoPrestamo = solicitudOriginal.TipoPrestamo.Nombre,
                    Ranking = rankingOriginal
                },
                SolicitudMejorada = new SolicitudResumenDto
                {
                    Id = nuevaSolicitud.Id,
                    Fecha = nuevaSolicitud.FechaSolicitud,
                    Estado = nuevaSolicitud.Estado,
                    Monto = nuevaSolicitud.Monto,
                    Plazo = nuevaSolicitud.Plazo,
                    CuotaEstimada = nuevaSolicitud.CuotaEstimadaCliente,
                    TipoPrestamo = solicitudOriginal.TipoPrestamo.Nombre,
                    Ranking = rankingNuevo
                }
            };
        }

        public async Task<ResultadoMejorasAplicadasDto> AplicarMejorasAutoAsync(int solicitudId)
        {
            var resultados = await _analisisService.ObtenerTodosPorSolicitudAsync(solicitudId);
            var mejoras = resultados
                .Where(r => r.MejorasSugeridas != null)
                .SelectMany(r => r.MejorasSugeridas)
                .Select(m => new MejoraAplicadaDto
                {
                    Variable = m.Variable,
                    ValorNuevo = m.ValorSugerido
                }).ToList();

            var dto = new AplicarMejorasSimuladasDto
            {
                SolicitudId = solicitudId,
                MejorasSeleccionadas = mejoras
            };

            return await AplicarMejorasAvanzadoAsync(dto);
        }
    }
}
