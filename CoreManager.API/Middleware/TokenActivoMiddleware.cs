using CoreManagerSP.API.CoreManager.Infrastructure.Configurations;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Threading.Tasks;

namespace CoreManagerSP.API.CoreManager.API.Middleware
{
    /// <summary>
    /// Middleware que valida si un token JWT está activo en la base de datos antes de permitir el acceso a rutas protegidas.
    /// Aplica tanto para tokens de administradores como de usuarios comunes.
    /// </summary>
    public class TokenActivoMiddleware
    {
        private readonly RequestDelegate _next;

        public TokenActivoMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        /// <summary>
        /// Invoca el middleware en cada request HTTP.
        /// Verifica si el token recibido en la cabecera Authorization está registrado y activo.
        /// </summary>
        /// <param name="context">Contexto HTTP de la solicitud.</param>
        /// <param name="dbContext">Instancia de la base de datos para validar el token.</param>
        public async Task InvokeAsync(HttpContext context, CoreManagerDbContext dbContext)
        {
            var authorizationHeader = context.Request.Headers["Authorization"].FirstOrDefault();

            if (!string.IsNullOrEmpty(authorizationHeader) && authorizationHeader.StartsWith("Bearer "))
            {
                var token = authorizationHeader.Substring("Bearer ".Length).Trim();

                try
                {
                    var tokenHandler = new JwtSecurityTokenHandler();
                    var jwtToken = tokenHandler.ReadJwtToken(token);

                    var userIdClaim = jwtToken.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier);
                    var roleClaim = jwtToken.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Role);

                    if (userIdClaim != null && int.TryParse(userIdClaim.Value, out int id) && roleClaim != null)
                    {
                        var esAdmin = roleClaim.Value == "Admin";

                        if (esAdmin)
                        {
                            var tokenAdmin = await dbContext.TokenAdmins
                                .FirstOrDefaultAsync(t => t.Token == token && t.AdminId == id);

                            if (tokenAdmin == null || !tokenAdmin.EstaActivo)
                            {
                                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                                await context.Response.WriteAsync("Token inactivo o no válido.");
                                return;
                            }
                        }
                        else
                        {
                            var tokenUsuario = await dbContext.TokenUsuarios
                                .FirstOrDefaultAsync(t => t.Token == token && t.UsuarioId == id);

                            if (tokenUsuario == null || !tokenUsuario.EstaActivo)
                            {
                                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                                await context.Response.WriteAsync("Token inactivo o no válido.");
                                return;
                            }
                        }
                    }
                }
                catch
                {
                    context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                    await context.Response.WriteAsync("Token inválido.");
                    return;
                }
            }

            await _next(context);
        }
    }

    /// <summary>
    /// Extensión para registrar el middleware de validación de tokens activos en la aplicación.
    /// </summary>
    public static class TokenActivoMiddlewareExtensions
    {
        /// <summary>
        /// Agrega el middleware de verificación de tokens activos a la tubería HTTP.
        /// </summary>
        /// <param name="builder">Aplicación de ASP.NET Core.</param>
        /// <returns>Constructor de aplicaciones con middleware configurado.</returns>
        public static IApplicationBuilder UseTokenActivoMiddleware(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<TokenActivoMiddleware>();
        }
    }
}
