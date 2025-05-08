using CoreManagerSP.API.CoreManager.Application.DTOs.Usuarios;
using CoreManagerSP.API.CoreManager.Application.Interfaces.Usuarios;
using CoreManagerSP.API.CoreManager.Domain.Entities;
using CoreManagerSP.API.CoreManager.Infrastructure.Configurations;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace CoreManagerSP.API.CoreManager.Infrastructure.Services.Usuarios
{
    public class UsuarioService : IUsuarioService
    {
        private readonly CoreManagerDbContext _context;

        public UsuarioService(CoreManagerDbContext context)
        {
            _context = context;
        }

        public async Task<Usuario> CrearAsync(UsuarioCreateDto dto)
        {
            // Validaciones previas
            if (string.IsNullOrWhiteSpace(dto.Correo) || !new EmailAddressAttribute().IsValid(dto.Correo))
                throw new InvalidOperationException("Correo inválido.");

            if (string.IsNullOrWhiteSpace(dto.Contrasena) || dto.Contrasena.Length < 8)
                throw new InvalidOperationException("La contraseña debe tener al menos 8 caracteres.");

            // Verificar si ya existe un correo igual
            var existeCorreo = await _context.Usuarios.AnyAsync(u => u.Correo == dto.Correo);
            if (existeCorreo)
                throw new InvalidOperationException("Ya existe un usuario con ese correo.");

            var usuario = new Usuario
            {
                Nombre = dto.Nombre,
                Apellido = dto.Apellido,
                Correo = dto.Correo,
                Ingreso = dto.Ingreso,
                NivelHistorialCrediticio = dto.NivelHistorialCrediticio,
                DeudasVigentes = dto.DeudasVigentes,
                CuotasMensualesComprometidas = dto.CuotasMensualesComprometidas,
                NumeroCreditosActivos = dto.NumeroCreditosActivos,
                HaTenidoMora = dto.HaTenidoMora,
                TiempoUltimoIncumplimiento = dto.TiempoUltimoIncumplimiento,
                TarjetaCredito = dto.TarjetaCredito,
                AniosHistorialCrediticio = dto.AniosHistorialCrediticio,
                FechaRegistro = DateTime.UtcNow
            };

            var hasher = new PasswordHasher<Usuario>();
            usuario.Contrasena = hasher.HashPassword(usuario, dto.Contrasena);

            _context.Usuarios.Add(usuario);
            await _context.SaveChangesAsync();

            return usuario;
        }




        public async Task<List<Usuario>> ObtenerTodosAsync()
        {
            return await _context.Usuarios.ToListAsync();
        }

        public async Task<Usuario?> ObtenerPorIdAsync(int id)
        {
            return await _context.Usuarios.FindAsync(id);
        }

        public async Task<bool> EliminarAsync(int id)
        {
            var usuario = await _context.Usuarios.FindAsync(id);
            if (usuario == null) return false;

            _context.Usuarios.Remove(usuario);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
