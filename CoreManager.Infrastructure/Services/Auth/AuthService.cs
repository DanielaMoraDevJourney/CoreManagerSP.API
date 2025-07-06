using CoreManagerSP.API.CoreManager.Application.DTOs.Logeo;
using CoreManagerSP.API.CoreManager.Application.Interfaces.Auth;
using CoreManagerSP.API.CoreManager.Domain.Entities;
using CoreManagerSP.API.CoreManager.Infrastructure.Configurations;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System.Threading.Tasks;

namespace CoreManagerSP.API.CoreManager.Infrastructure.Services.Auth
{
    public class AuthService : IAuthService
    {
        private readonly CoreManagerDbContext _context;
        private readonly IJwtTokenGenerator _tokenGenerator;

        public AuthService(
            CoreManagerDbContext context,
            IJwtTokenGenerator tokenGenerator)
        {
            _context = context;
            _tokenGenerator = tokenGenerator;
        }

        public async Task<LoginResponseDto?> LoginAsync(LoginRequestDto dto)
        {
            var usuario = await _context.Usuarios
                .FirstOrDefaultAsync(u => u.Correo == dto.Correo);
            if (usuario == null)
                return null;

            var hasher = new PasswordHasher<Usuario>();
            var verify = hasher.VerifyHashedPassword(usuario, usuario.Contrasena, dto.Contrasena);
            if (verify == PasswordVerificationResult.Failed)
                return null;

            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, usuario.Id.ToString()),
                new Claim(ClaimTypes.Email,           usuario.Correo),
                new Claim(ClaimTypes.Name,            usuario.Nombre),
                new Claim(ClaimTypes.Role,            "Usuario")
            };

            // Delegamos la creación del JWT a la abstracción
            var tokenString = _tokenGenerator.GenerateToken(claims, out var expiracion);

            _context.TokenUsuarios.Add(new TokenUsuario
            {
                UsuarioId = usuario.Id,
                Token = tokenString,
                FechaExpiracion = expiracion,
                EstaActivo = true
            });

            await _context.SaveChangesAsync();

            return new LoginResponseDto
            {
                Token = tokenString,
                UsuarioId = usuario.Id,
                Nombre = usuario.Nombre,
                Correo = usuario.Correo
            };
        }

        public async Task<bool> RegistrarUsuarioAsync(RegisterRequestDto dto)
        {
            var existe = await _context.Usuarios
                .AnyAsync(u => u.Correo == dto.Correo);
            if (existe)
                return false;

            var hasher = new PasswordHasher<Usuario>();
            var nuevo = new Usuario
            {
                Nombre = dto.Nombre,
                Apellido = dto.Apellido,
                Correo = dto.Correo,
                Ingreso = 0,
                TarjetaCredito = false,
                HaTenidoMora = false,
                AniosHistorialCrediticio = 0
            };

            nuevo.Contrasena = hasher.HashPassword(nuevo, dto.Contrasena);

            _context.Usuarios.Add(nuevo);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
