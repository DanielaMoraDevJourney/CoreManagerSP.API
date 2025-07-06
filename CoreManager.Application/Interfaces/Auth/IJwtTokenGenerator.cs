using System.Security.Claims;

namespace CoreManagerSP.API.CoreManager.Application.Interfaces.Auth
{

    public interface IJwtTokenGenerator
    {
        /// <summary>
        /// Genera un token JWT a partir de una colección de claims.
        /// Devuelve el string del token y por out la fecha de expiración.
        /// </summary>
        string GenerateToken(IEnumerable<Claim> claims, out DateTime expiration);
    }
}
