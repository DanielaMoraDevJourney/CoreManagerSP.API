using CoreManagerSP.API.CoreManager.Application.DTOs.EntidadesFinancieras;
using CoreManagerSP.API.CoreManager.Application.DTOs.Prestamo;
using CoreManagerSP.API.CoreManager.Application.DTOs.Prestamo.Analisis;
using CoreManagerSP.API.CoreManager.Application.DTOs.Prestamo.Historial;
using CoreManagerSP.API.CoreManager.Application.DTOs.Prestamo.Mejoras;
using CoreManagerSP.API.CoreManager.Application.DTOs.Prestamo.Sugerencias;
using CoreManagerSP.API.CoreManager.Application.DTOs.Prestamo.Sugerencias.CoreManagerSP.API.CoreManager.Application.DTOs.Prestamo.Analisis;
using CoreManagerSP.API.CoreManager.Application.Interfaces.Prestamo;
using CoreManagerSP.API.CoreManager.Domain.Entities;
using CoreManagerSP.API.CoreManager.Infrastructure.Configurations;
using Microsoft.EntityFrameworkCore;

namespace CoreManagerSP.API.CoreManager.Infrastructure.Services.Prestamo
{
    public class SolicitudPrestamoService : ISolicitudPrestamoService
    {
        private readonly CoreManagerDbContext _context;
        private readonly ILogger<SolicitudPrestamoService> _logger;

        public SolicitudPrestamoService(CoreManagerDbContext context, ILogger<SolicitudPrestamoService> logger)
        {
            _context = context;
            _logger = logger;
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


        public async Task<SolicitudPrestamo?> ObtenerPorIdAsync(int id)
        {
            return await _context.Solicitudes
                .Include(s => s.AnalisisResultados)
                .ThenInclude(r => r.EntidadFinanciera)
                .Include(s => s.TipoPrestamo)
                .Include(s => s.Usuario)
                .FirstOrDefaultAsync(s => s.Id == id);
        }


        public async Task<List<SolicitudPrestamo>> ObtenerPorUsuarioAsync(int usuarioId)
        {
            return await _context.Solicitudes
                .Where(s => s.UsuarioId == usuarioId)
                .Include(s => s.TipoPrestamo)
                .Include(s => s.AnalisisResultados)
                    .ThenInclude(r => r.EntidadFinanciera)
                .OrderByDescending(s => s.FechaSolicitud)
                .ToListAsync();
        }

        private decimal CalcularCuota(decimal monto, int plazoMeses, decimal tasaInteresAnual)
        {
            if (plazoMeses <= 0 || tasaInteresAnual <= 0) return 0;

            var tasaMensual = tasaInteresAnual / 12 / 100;
            return monto * tasaMensual / (1 - (decimal)Math.Pow(1 + (double)tasaMensual, -plazoMeses));
        }

        private decimal CalcularProbabilidad(EntidadFinanciera entidad, Usuario usuario, SolicitudPrestamoCreateDto dto)
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

        private bool EsAprobado(EntidadFinanciera entidad, Usuario usuario, SolicitudPrestamoCreateDto dto)
        {
            var cuota = CalcularCuota(dto.Monto, dto.Plazo, entidad.TasaInteres);

            bool cumpleIngreso = usuario.Ingreso >= entidad.IngresoMinimo;
            bool cumpleAntiguedad = usuario.AniosHistorialCrediticio >= entidad.AntiguedadHistorialMinima;
            bool sinMoraOEntidadAcepta = entidad.AceptaMora || !usuario.HaTenidoMora;
            bool cumpleTarjeta = !entidad.RequiereTarjetaCredito || usuario.TarjetaCredito;
            bool cumpleRelacionCuotaIngreso = usuario.Ingreso > 0 && (cuota / usuario.Ingreso) <= entidad.RelacionCuotaIngresoMaxima;

            return cumpleIngreso &&
                   cumpleAntiguedad &&
                   sinMoraOEntidadAcepta &&
                   cumpleTarjeta &&
                   cumpleRelacionCuotaIngreso;
        }

        ////---------------------------------------------------------------------------------------------------------------////////////////

        private async Task GenerarAnalisisYMejorasAsync(SolicitudPrestamo solicitud, Usuario usuario, decimal monto, int plazo, int tipoPrestamoId, bool esReanalisis = false)
        {
            var entidades = await _context.EntidadesFinancieras
                .Include(e => e.EntidadesTipoPrestamo)
                .ToListAsync();

            foreach (var entidad in entidades)
            {
                if (!entidad.EntidadesTipoPrestamo.Any(e => e.TipoPrestamoId == tipoPrestamoId)) continue;

                var cuota = CalcularCuota(monto, plazo, entidad.TasaInteres);

                var resultado = new AnalisisResultado
                {
                    SolicitudPrestamoId = solicitud.Id,
                    EntidadFinancieraId = entidad.Id,
                    CuotaMensualEstimada = cuota,
                    EsApto = EsAprobado(entidad, usuario, new SolicitudPrestamoCreateDto
                    {
                        Monto = monto,
                        Plazo = plazo,
                        TipoPrestamoId = tipoPrestamoId
                    }),
                    ProbabilidadAprobacion = CalcularProbabilidad(entidad, usuario, new SolicitudPrestamoCreateDto
                    {
                        Monto = monto,
                        Plazo = plazo,
                        TipoPrestamoId = tipoPrestamoId
                    }),
                    MensajeResumen = esReanalisis ? "Reanálisis tras aplicar mejoras" : "Resultado automático generado"
                };

                _context.AnalisisResultados.Add(resultado);
                await _context.SaveChangesAsync(); // Necesario para generar el ID del análisis

                var mejoras = new List<MejoraSugerida>();

                if (usuario.Ingreso < entidad.IngresoMinimo)
                {
                    mejoras.Add(new MejoraSugerida
                    {
                        AnalisisResultadoId = resultado.Id,
                        Variable = "Ingreso",
                        ValorSugerido = entidad.IngresoMinimo.ToString("F2"),
                        Descripcion = "Aumenta tus ingresos mensuales para cumplir el mínimo requerido.",
                        ImpactoEstimado = "Alta",
                        EsObligatoria = true,
                        Prioridad = 1
                    });
                }

                if (usuario.HaTenidoMora && !entidad.AceptaMora)
                {
                    mejoras.Add(new MejoraSugerida
                    {
                        AnalisisResultadoId = resultado.Id,
                        Variable = "Historial de mora",
                        ValorSugerido = "Sin mora en los últimos 12 meses",
                        Descripcion = "Evita caer en mora antes de volver a aplicar.",
                        ImpactoEstimado = "Alta",
                        EsObligatoria = true,
                        Prioridad = 2
                    });
                }

                if (entidad.RequiereTarjetaCredito && !usuario.TarjetaCredito)
                {
                    mejoras.Add(new MejoraSugerida
                    {
                        AnalisisResultadoId = resultado.Id,
                        Variable = "Tarjeta de crédito",
                        ValorSugerido = "Obtener tarjeta de crédito",
                        Descripcion = "Adquiere una tarjeta de crédito para cumplir el requisito.",
                        ImpactoEstimado = "Media",
                        EsObligatoria = false,
                        Prioridad = 3
                    });
                }

                if (usuario.Ingreso > 0 && cuota / usuario.Ingreso > entidad.RelacionCuotaIngresoMaxima)
                {
                    mejoras.Add(new MejoraSugerida
                    {
                        AnalisisResultadoId = resultado.Id,
                        Variable = "Cuota mensual",
                        ValorSugerido = "Reducir monto o ampliar plazo",
                        Descripcion = "Reduce el monto solicitado o alarga el plazo para disminuir la cuota mensual.",
                        ImpactoEstimado = "Alta",
                        EsObligatoria = true,
                        Prioridad = 4
                    });
                }

                _context.MejorasSugeridas.AddRange(mejoras);
            }

            await _context.SaveChangesAsync();
        }

        public async Task<SolicitudPrestamo> CrearAsync(SolicitudPrestamoCreateDto dto)
        {
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

            var usuario = await _context.Usuarios.FindAsync(dto.UsuarioId);
            var entidades = await _context.EntidadesFinancieras
                .Include(e => e.EntidadesTipoPrestamo)
                .Where(e => e.EntidadesTipoPrestamo.Any(t => t.TipoPrestamoId == dto.TipoPrestamoId))
                .ToListAsync();

            // 3. Generar análisis + mejoras para cada entidad
            foreach (var entidad in entidades)
            {
                var resultado = new AnalisisResultado
                {
                    SolicitudPrestamoId = solicitud.Id,
                    EntidadFinancieraId = entidad.Id,
                    CuotaMensualEstimada = CalcularCuota(dto.Monto, dto.Plazo, entidad.TasaInteres),
                    EsApto = EsAprobado(entidad, usuario, dto),
                    ProbabilidadAprobacion = CalcularProbabilidad(entidad, usuario, dto),
                    MensajeResumen = "Resultado automático generado"
                };

                _context.AnalisisResultados.Add(resultado);
                await _context.SaveChangesAsync(); // Para obtener el ID del resultado

                await GenerarAnalisisYMejorasAsync(solicitud, usuario, dto.Monto, dto.Plazo, dto.TipoPrestamoId);

            }

            await _context.SaveChangesAsync();
            return solicitud;
        }


        ///------------------------------------------------------------------------------------------------------------------------//

        public async Task<List<HistorialSimulacionDto>> ObtenerHistorialPorUsuarioAsync(int usuarioId)
        {
            var solicitudes = await _context.Solicitudes
                .Where(s => s.UsuarioId == usuarioId)
                .Include(s => s.TipoPrestamo)
                .Include(s => s.AnalisisResultados)
                    .ThenInclude(r => r.EntidadFinanciera)
                .OrderByDescending(s => s.FechaSolicitud)
                .ToListAsync();

            var historial = solicitudes.Select(s => new HistorialSimulacionDto
            {
                SolicitudId = s.Id,
                Fecha = s.FechaSolicitud,
                TipoPrestamo = s.TipoPrestamo.Nombre,
                Monto = s.Monto,
                Plazo = s.Plazo,
                Estado = s.Estado,
                Resultados = s.AnalisisResultados.Select(r => new HistorialResultadoEntidadDto
                {
                    NombreEntidad = r.EntidadFinanciera.Nombre,
                    Probabilidad = r.ProbabilidadAprobacion,
                    EsApto = r.EsApto
                }).ToList()
            }).ToList();

            return historial;
        }

        public async Task<bool> CrearSimulacionCompletaAsync(SimulacionCompletaDto dto)
        {
            var usuario = await _context.Usuarios.FindAsync(dto.UsuarioId);
            if (usuario == null) return false;

            // Actualiza el perfil financiero del usuario
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

            // Crea la nueva solicitud
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

            // Genera el análisis y sugerencias de mejora
            await GenerarAnalisisYMejorasAsync(solicitud, usuario, dto.Monto, dto.Plazo, dto.TipoPrestamoId);

            return true;
        }

        ///------------------------------------------------------------------------------------------------------------------------//

        public async Task<AnalisisDetalleDto?> ObtenerAnalisisPorEntidadAsync(int solicitudId, int entidadId)
        {
            var resultado = await _context.AnalisisResultados
                .Include(a => a.EntidadFinanciera)
                .Include(a => a.SolicitudPrestamo)
                    .ThenInclude(s => s.Usuario)
                .FirstOrDefaultAsync(a =>
                    a.SolicitudPrestamoId == solicitudId &&
                    a.EntidadFinancieraId == entidadId);

            if (resultado == null) return null;

            var entidad = resultado.EntidadFinanciera;
            var usuario = resultado.SolicitudPrestamo.Usuario;
            var cuota = resultado.CuotaMensualEstimada;

            var criterios = new List<CriterioEvaluadoDto>
            {
                new CriterioEvaluadoDto
                {
                    Criterio         = "Ingreso mínimo",
                    ValorUsuario     = usuario.Ingreso.ToString("C"),
                    ValorRequerido   = entidad.IngresoMinimo.ToString("C"),
                    Cumple           = usuario.Ingreso >= entidad.IngresoMinimo
                },
                new CriterioEvaluadoDto
                {
                    Criterio         = "Antigüedad historial crediticio",
                    ValorUsuario     = $"{usuario.AniosHistorialCrediticio} años",
                    ValorRequerido   = $"{entidad.AntiguedadHistorialMinima} años",
                    Cumple           = usuario.AniosHistorialCrediticio >= entidad.AntiguedadHistorialMinima
                },
                new CriterioEvaluadoDto
                {
                    Criterio         = "Relación cuota-ingreso",
                    ValorUsuario     = (cuota / usuario.Ingreso).ToString("P"),
                    ValorRequerido   = entidad.RelacionCuotaIngresoMaxima.ToString("P"),
                    Cumple           = (cuota / usuario.Ingreso) <= entidad.RelacionCuotaIngresoMaxima
                },
                new CriterioEvaluadoDto
                {
                    Criterio         = "¿Permite mora previa?",
                    ValorUsuario     = usuario.HaTenidoMora ? "Sí" : "No",
                    ValorRequerido   = entidad.AceptaMora ? "Sí" : "No",
                    Cumple           = entidad.AceptaMora || !usuario.HaTenidoMora
                },
                new CriterioEvaluadoDto
                {
                    Criterio         = "¿Requiere tarjeta de crédito?",
                    ValorUsuario     = usuario.TarjetaCredito ? "Sí" : "No",
                    ValorRequerido   = entidad.RequiereTarjetaCredito ? "Sí" : "No",
                    Cumple           = !entidad.RequiereTarjetaCredito || usuario.TarjetaCredito
                }
            };

            return new AnalisisDetalleDto
            {
                NombreEntidad = entidad.Nombre,
                ProbabilidadAprobacion = resultado.ProbabilidadAprobacion,
                CuotaMensualEstimada = cuota,
                EsApto = resultado.EsApto,
                Criterios = criterios
            };
        }

        public async Task<List<AnalisisResultadoDto>> ObtenerRankingPorSolicitudAsync(int solicitudId)
        {
            var resultados = await _context.AnalisisResultados
                .Include(a => a.EntidadFinanciera)
                .Where(a => a.SolicitudPrestamoId == solicitudId)
                .OrderByDescending(a => a.ProbabilidadAprobacion)
                .ToListAsync();

            return resultados.Select(a => new AnalisisResultadoDto
            {
                EntidadId = a.EntidadFinancieraId,
                NombreEntidad = a.EntidadFinanciera.Nombre,
                ProbabilidadAprobacion = a.ProbabilidadAprobacion,
                CuotaMensualEstimada = a.CuotaMensualEstimada,
                EsApto = a.EsApto
            }).ToList();
        }

        ///-----------------------------------------------------------------------------------------------------------------------
        ///

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
                string valorAnterior = "";

                try
                {
                    switch (variable)
                    {
                        case "Ingreso":
                            valorAnterior = usuario.Ingreso.ToString();
                            usuario.Ingreso = decimal.Parse(nuevoValor);
                            break;

                        case "TarjetaCredito":
                            valorAnterior = usuario.TarjetaCredito.ToString();
                            usuario.TarjetaCredito = bool.Parse(nuevoValor);
                            break;

                        case "AniosHistorialCrediticio":
                            valorAnterior = usuario.AniosHistorialCrediticio.ToString();
                            usuario.AniosHistorialCrediticio = int.Parse(nuevoValor);
                            break;

                        case "DeudasVigentes":
                            valorAnterior = usuario.DeudasVigentes.ToString();
                            usuario.DeudasVigentes = decimal.Parse(nuevoValor);
                            break;

                        case "CuotasMensualesComprometidas":
                            valorAnterior = usuario.CuotasMensualesComprometidas.ToString();
                            usuario.CuotasMensualesComprometidas = decimal.Parse(nuevoValor);
                            break;

                        case "NumeroCreditosActivos":
                            valorAnterior = usuario.NumeroCreditosActivos.ToString();
                            usuario.NumeroCreditosActivos = int.Parse(nuevoValor);
                            break;

                        case "TiempoUltimoIncumplimiento":
                            valorAnterior = usuario.TiempoUltimoIncumplimiento;
                            usuario.TiempoUltimoIncumplimiento = nuevoValor;
                            break;

                        default:
                            continue;
                    }

                    cambios.Add(new HistorialMejoras
                    {
                        SolicitudPrestamoId = solicitud.Id,
                        VariableModificada = variable,
                        ValorAnterior = valorAnterior,
                        ValorNuevo = nuevoValor,
                        FechaAplicacion = DateTime.UtcNow
                    });
                }
                catch
                {
                    _logger.LogWarning($"Error al aplicar mejora: {variable} con valor '{nuevoValor}'");
                    continue;
                }
            }

            _context.HistorialMejoras.AddRange(cambios);
            _context.Usuarios.Update(usuario);

            // Borrar análisis y mejoras previas
            var resultadosAnteriores = await _context.AnalisisResultados
                .Where(a => a.SolicitudPrestamoId == solicitud.Id)
                .Include(a => a.MejorasSugeridas)
                .ToListAsync();

            _context.MejorasSugeridas.RemoveRange(resultadosAnteriores.SelectMany(r => r.MejorasSugeridas));
            _context.AnalisisResultados.RemoveRange(resultadosAnteriores);

            await _context.SaveChangesAsync();

            // Volver a generar análisis con nuevo perfil
            await GenerarAnalisisYMejorasAsync(
                solicitud,
                usuario,
                solicitud.Monto,
                solicitud.Plazo,
                solicitud.TipoPrestamoId,
                esReanalisis: true
            );

            return true;
        }

        ///------------------------------------------------------------------------------------------------------------------------//

        public async Task<AnalisisEntidadCompletoDto?> ObtenerAnalisisCompletoPorEntidadAsync(int solicitudId, int entidadId)
        {
            var detalle = await ObtenerAnalisisPorEntidadAsync(solicitudId, entidadId);
            var mejoras = await ObtenerMejorasPorEntidadAsync(solicitudId, entidadId);

            if (detalle == null) return null;

            return new AnalisisEntidadCompletoDto
            {
                NombreEntidad = detalle.NombreEntidad,
                ProbabilidadAprobacion = detalle.ProbabilidadAprobacion,
                CuotaMensualEstimada = detalle.CuotaMensualEstimada,
                EsApto = detalle.EsApto,
                Criterios = detalle.Criterios,
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
            var comparativas = new List<AnalisisEntidadCompletoDto>();

            foreach (var entidadId in entidadIds)
            {
                var analisis = await ObtenerAnalisisCompletoPorEntidadAsync(solicitudId, entidadId);
                if (analisis != null)
                    comparativas.Add(analisis);
            }

            return comparativas;
        }


        public async Task<ResultadoMejorasAplicadasDto> AplicarMejorasAvanzadoAsync(AplicarMejorasSimuladasDto dto)
        {
            var solicitudOriginal = await _context.Solicitudes
                .Include(s => s.Usuario)
                .Include(s => s.TipoPrestamo)
                .FirstOrDefaultAsync(s => s.Id == dto.SolicitudId);

            if (solicitudOriginal == null) throw new Exception("Solicitud no encontrada.");

            var usuario = solicitudOriginal.Usuario;
            var perfilActualizado = new Dictionary<string, string>();

            foreach (var mejora in dto.MejorasSeleccionadas)
            {
                switch (mejora.Variable)
                {
                    case "Ingreso":
                        perfilActualizado["Ingreso"] = mejora.ValorNuevo;
                        usuario.Ingreso = decimal.Parse(mejora.ValorNuevo); break;
                    case "TarjetaCredito":
                        perfilActualizado["TarjetaCredito"] = mejora.ValorNuevo;
                        usuario.TarjetaCredito = bool.Parse(mejora.ValorNuevo); break;
                    case "AniosHistorialCrediticio":
                        perfilActualizado["AniosHistorialCrediticio"] = mejora.ValorNuevo;
                        usuario.AniosHistorialCrediticio = int.Parse(mejora.ValorNuevo); break;
                    case "DeudasVigentes":
                        perfilActualizado["DeudasVigentes"] = mejora.ValorNuevo;
                        usuario.DeudasVigentes = decimal.Parse(mejora.ValorNuevo); break;
                    case "CuotasMensualesComprometidas":
                        perfilActualizado["CuotasMensualesComprometidas"] = mejora.ValorNuevo;
                        usuario.CuotasMensualesComprometidas = decimal.Parse(mejora.ValorNuevo); break;
                    case "NumeroCreditosActivos":
                        perfilActualizado["NumeroCreditosActivos"] = mejora.ValorNuevo;
                        usuario.NumeroCreditosActivos = int.Parse(mejora.ValorNuevo); break;
                    case "TiempoUltimoIncumplimiento":
                        perfilActualizado["TiempoUltimoIncumplimiento"] = mejora.ValorNuevo;
                        usuario.TiempoUltimoIncumplimiento = mejora.ValorNuevo; break;
                }
            }

            _context.Usuarios.Update(usuario);
            await _context.SaveChangesAsync();

            // Obtener ranking de la solicitud original
            var rankingOriginal = await ObtenerRankingPorSolicitudAsync(dto.SolicitudId);

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

            await GenerarAnalisisYMejorasAsync(
                nuevaSolicitud,
                usuario,
                nuevaSolicitud.Monto,
                nuevaSolicitud.Plazo,
                nuevaSolicitud.TipoPrestamoId,
                true
            );

            var rankingNuevo = await ObtenerRankingPorSolicitudAsync(nuevaSolicitud.Id);

            return new ResultadoMejorasAplicadasDto
            {
                Mensaje = "Mejoras aplicadas y nueva simulación generada.",
                Fecha = nuevaSolicitud.FechaSolicitud,
                PerfilActualizado = perfilActualizado,

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




    }
}
