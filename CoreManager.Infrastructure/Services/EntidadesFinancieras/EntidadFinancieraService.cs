using CoreManagerSP.API.CoreManager.Application.DTOs.EntidadFinanciera;
using CoreManagerSP.API.CoreManager.Application.Interfaces.EntidadesFinancieras;
using CoreManagerSP.API.CoreManager.Domain.Entities;
using CoreManagerSP.API.CoreManager.Infrastructure.Configurations;
using Microsoft.EntityFrameworkCore;

namespace CoreManagerSP.API.CoreManager.Infrastructure.Services.EntidadesFinancieras
{
    public class EntidadFinancieraService : IEntidadFinancieraService
    {
        private readonly CoreManagerDbContext _context;

        public EntidadFinancieraService(CoreManagerDbContext context)
        {
            _context = context;
        }

        public async Task<List<EntidadFinanciera>> ObtenerTodosAsync()
        {
            return await _context.EntidadesFinancieras
                .Include(e => e.EntidadesTipoPrestamo)
                    .ThenInclude(et => et.TipoPrestamo)
                .ToListAsync();
        }

        public async Task<EntidadFinanciera?> ObtenerPorIdAsync(int id)
        {
            return await _context.EntidadesFinancieras
                .Include(e => e.EntidadesTipoPrestamo)
                    .ThenInclude(et => et.TipoPrestamo)
                .FirstOrDefaultAsync(e => e.Id == id);
        }

        public async Task<EntidadFinanciera> CrearAsync(EntidadFinancieraCreateDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.Nombre))
                throw new ArgumentException("El nombre de la entidad financiera es obligatorio.");

            var existeNombre = await _context.EntidadesFinancieras
                .AnyAsync(e => e.Nombre.ToLower() == dto.Nombre.ToLower());

            if (existeNombre)
                throw new InvalidOperationException("Ya existe una entidad financiera con ese nombre.");

            var entidad = new EntidadFinanciera
            {
                Nombre = dto.Nombre,
                TasaInteres = dto.TasaInteres,
                IngresoMinimo = dto.IngresoMinimo,
                RelacionCuotaIngresoMaxima = dto.RelacionCuotaIngresoMaxima,
                AntiguedadHistorialMinima = dto.AntiguedadHistorialMinima,
                AceptaMora = dto.AceptaMora,
                RequiereTarjetaCredito = dto.RequiereTarjetaCredito
            };

            _context.EntidadesFinancieras.Add(entidad);
            await _context.SaveChangesAsync();

            if (dto.TiposPrestamoIds != null && dto.TiposPrestamoIds.Any())
            {
                foreach (var tipoId in dto.TiposPrestamoIds)
                {
                    var relacion = new EntidadTipoPrestamo
                    {
                        EntidadFinancieraId = entidad.Id,
                        TipoPrestamoId = tipoId
                    };
                    _context.EntidadesTipoPrestamo.Add(relacion);
                }

                await _context.SaveChangesAsync();
            }

            return entidad;
        }

        public async Task<bool> EliminarAsync(int id)
        {
            var entidad = await _context.EntidadesFinancieras
                .Include(e => e.EntidadesTipoPrestamo)
                .FirstOrDefaultAsync(e => e.Id == id);

            if (entidad == null)
                return false;

            // Eliminar relaciones antes de la entidad principal
            _context.EntidadesTipoPrestamo.RemoveRange(entidad.EntidadesTipoPrestamo);
            _context.EntidadesFinancieras.Remove(entidad);

            await _context.SaveChangesAsync();
            return true;
        }
    }
}
