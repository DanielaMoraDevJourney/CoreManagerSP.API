using CoreManagerSP.API.CoreManager.Application.DTOs.Logeo;
using CoreManagerSP.API.CoreManager.Application.Interfaces.Auth;
using CoreManagerSP.API.CoreManager.Domain.Entities;
using CoreManagerSP.API.CoreManager.Infrastructure.Configurations;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace CoreManagerSP.API.CoreManager.Infrastructure.Services.Auth
{
    public class AuthService : IAuthService
    {
        private readonly CoreManagerDbContext _context;
        private readonly IConfiguration _config;

        public AuthService(CoreManagerDbContext context, IConfiguration config)
        {
            _context = context;
            _config = config;
        }

        public async Task<LoginResponseDto?> LoginAsync(LoginRequestDto dto)
        {
            var usuario = await _context.Usuarios.FirstOrDefaultAsync(u => u.Correo == dto.Correo);
            if (usuario == null) return null;

            var hasher = new PasswordHasher<Usuario>();
            var resultado = hasher.VerifyHashedPassword(usuario, usuario.Contrasena, dto.Contrasena);
            if (resultado == PasswordVerificationResult.Failed) return null;

            // Generar token con rol "Usuario"
            var key = Encoding.ASCII.GetBytes(_config["Jwt:Key"]);
            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, usuario.Id.ToString()),
                new Claim(ClaimTypes.Email, usuario.Correo),
                new Claim(ClaimTypes.Name, usuario.Nombre),
                new Claim(ClaimTypes.Role, "Usuario") // <- Rol agregado
            };

            var token = new JwtSecurityToken(
                issuer: _config["Jwt:Issuer"],
                audience: _config["Jwt:Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddHours(3),
                signingCredentials: new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256)
            );

            var tokenString = new JwtSecurityTokenHandler().WriteToken(token);

            // Guardar token en base de datos
            _context.TokenUsuarios.Add(new TokenUsuario
            {
                UsuarioId = usuario.Id,
                Token = tokenString,
                FechaExpiracion = token.ValidTo,
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
            var existe = await _context.Usuarios.AnyAsync(u => u.Correo == dto.Correo);
            if (existe) return false;

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
