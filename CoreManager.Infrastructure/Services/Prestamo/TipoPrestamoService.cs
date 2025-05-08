using CoreManagerSP.API.CoreManager.Application.DTOs.Prestamo;
using CoreManagerSP.API.CoreManager.Application.Interfaces.Prestamo;
using CoreManagerSP.API.CoreManager.Domain.Entities;
using CoreManagerSP.API.CoreManager.Infrastructure.Configurations;
using Microsoft.EntityFrameworkCore;

namespace CoreManagerSP.API.CoreManager.Infrastructure.Services.Prestamo
{
    public class TipoPrestamoService : ITipoPrestamoService
    {
        private readonly CoreManagerDbContext _context;

        public TipoPrestamoService(CoreManagerDbContext context)
        {
            _context = context;
        }

        public async Task<List<TipoPrestamo>> ObtenerTodosAsync()
        {
            return await _context.TiposPrestamo.ToListAsync();
        }

        public async Task<TipoPrestamo?> ObtenerPorIdAsync(int id)
        {
            return await _context.TiposPrestamo.FindAsync(id);
        }

        public async Task<TipoPrestamo> CrearAsync(TipoPrestamoCreateDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.Nombre))
                throw new ArgumentException("El nombre del tipo de préstamo es obligatorio.");

            if (dto.TasaBase <= 0)
                throw new ArgumentException("La tasa base debe ser mayor a cero.");

            if (dto.CuotaMinima <= 0)
                throw new ArgumentException("La cuota mínima debe ser mayor a cero.");

            var existe = await _context.TiposPrestamo
                .AnyAsync(t => t.Nombre.ToLower() == dto.Nombre.ToLower());

            if (existe)
                throw new InvalidOperationException("Ya existe un tipo de préstamo con ese nombre.");

            var tipo = new TipoPrestamo
            {
                Nombre = dto.Nombre,
                Descripcion = dto.Descripcion,
                TipoGeneral = dto.TipoGeneral,
                TasaBase = dto.TasaBase,
                CuotaMinima = dto.CuotaMinima,
                OtrosRequisitos = dto.OtrosRequisitos
            };

            _context.TiposPrestamo.Add(tipo);
            await _context.SaveChangesAsync();
            return tipo;
        }

        public async Task<bool> EliminarAsync(int id)
        {
            var tipo = await _context.TiposPrestamo.FindAsync(id);
            if (tipo == null) return false;

            _context.TiposPrestamo.Remove(tipo);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
