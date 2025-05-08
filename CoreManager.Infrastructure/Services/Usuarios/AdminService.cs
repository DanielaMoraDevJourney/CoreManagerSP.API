using CoreManagerSP.API.CoreManager.Application.DTOs.Logeo;
using CoreManagerSP.API.CoreManager.Application.DTOs.Usuarios;
using CoreManagerSP.API.CoreManager.Application.Interfaces.Usuarios;
using CoreManagerSP.API.CoreManager.Domain.Entities;
using CoreManagerSP.API.CoreManager.Infrastructure.Configurations;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace CoreManagerSP.API.CoreManager.Infrastructure.Services.Usuarios
{
    public class AdminService : IAdminService
    {
        private readonly CoreManagerDbContext _context;
        private readonly PasswordHasher<Admin> _hasher;
        private readonly IConfiguration _config;

        public AdminService(CoreManagerDbContext context, IConfiguration config)
        {
            _context = context;
            _config = config;
            _hasher = new PasswordHasher<Admin>();
        }

        public async Task<AdminResponseDto> CrearAsync(AdminCreateDto dto)
        {
            var existeCorreo = await _context.Administradores.AnyAsync(a => a.Correo == dto.Correo);
            if (existeCorreo)
                throw new InvalidOperationException("Ya existe un administrador con ese correo.");

            if (string.IsNullOrWhiteSpace(dto.Contrasena) || dto.Contrasena.Length < 8)
                throw new InvalidOperationException("La contraseña debe tener al menos 8 caracteres.");

            var admin = new Admin
            {
                Nombre = dto.Nombre,
                Correo = dto.Correo,
                FechaRegistro = DateTime.UtcNow,
                ContrasenaHash = _hasher.HashPassword(null, dto.Contrasena)
            };

            _context.Administradores.Add(admin);
            await _context.SaveChangesAsync();

            return new AdminResponseDto
            {
                Id = admin.Id,
                Nombre = admin.Nombre,
                Correo = admin.Correo,
                FechaRegistro = admin.FechaRegistro,
                Rol = "Admin"
            };
        }


        public async Task<bool> EliminarAsync(int id)
        {
            var admin = await _context.Administradores.FindAsync(id);
            if (admin == null) return false;

            _context.Administradores.Remove(admin);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<List<AdminResponseDto>> ObtenerTodosAsync()
        {
            var admins = await _context.Administradores.ToListAsync();
            return admins.Select(MapToResponse).ToList();
        }

        public async Task<AdminResponseDto?> ObtenerPorIdAsync(int id)
        {
            var admin = await _context.Administradores.FindAsync(id);
            return admin == null ? null : MapToResponse(admin);
        }

        public async Task<AdminLoginResponseDto?> LoginAsync(AdminLoginDto dto)
        {
            var admin = await _context.Administradores.FirstOrDefaultAsync(a => a.Correo == dto.Correo);
            if (admin == null) return null;

            var isValid = _hasher.VerifyHashedPassword(admin, admin.ContrasenaHash, dto.Contrasena);
            if (isValid != PasswordVerificationResult.Success) return null;

            var tokenString = GenerarToken(admin, out DateTime expiracion);

            _context.TokenAdmins.Add(new TokenAdmin
            {
                AdminId = admin.Id,
                Token = tokenString,
                FechaExpiracion = expiracion,
                EstaActivo = true
            });

            await _context.SaveChangesAsync();

            return new AdminLoginResponseDto
            {
                Id = admin.Id,
                Nombre = admin.Nombre,
                Correo = admin.Correo,
                Rol = admin.Rol,
                Token = tokenString
            };
        }



        private AdminResponseDto MapToResponse(Admin admin) => new()
        {
            Id = admin.Id,
            Nombre = admin.Nombre,
            Correo = admin.Correo,
            FechaRegistro = admin.FechaRegistro,
            Rol = "Admin"
        };

        private string GenerarToken(Admin admin, out DateTime expiracion)
        {
            var key = Encoding.UTF8.GetBytes(_config["Jwt:Key"]);
            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, admin.Id.ToString()),
                new Claim(ClaimTypes.Email, admin.Correo),
                new Claim(ClaimTypes.Name, admin.Nombre),
                new Claim(ClaimTypes.Role, "Admin")
            };

            expiracion = DateTime.UtcNow.AddHours(3);

            var credentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256);
            var token = new JwtSecurityToken(
                issuer: _config["Jwt:Issuer"],
                audience: _config["Jwt:Audience"],
                claims: claims,
                expires: expiracion,
                signingCredentials: credentials
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

    }
}
