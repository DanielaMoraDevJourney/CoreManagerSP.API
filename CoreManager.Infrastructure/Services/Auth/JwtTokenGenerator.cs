using CoreManagerSP.API.CoreManager.Application.Interfaces.Auth;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace CoreManagerSP.API.CoreManager.Infrastructure.Services.Auth
{

    public class JwtTokenGenerator : IJwtTokenGenerator
    {
        private readonly byte[] _key;
        private readonly string _issuer;
        private readonly string _audience;

        public JwtTokenGenerator(IConfiguration config)
        {
            _key = Encoding.UTF8.GetBytes(config["Jwt:Key"]);
            _issuer = config["Jwt:Issuer"];
            _audience = config["Jwt:Audience"];
        }

        public string GenerateToken(IEnumerable<Claim> claims, out DateTime expiration)
        {
            expiration = DateTime.UtcNow.AddHours(3);

            var creds = new SigningCredentials(new SymmetricSecurityKey(_key), SecurityAlgorithms.HmacSha256);
            var token = new JwtSecurityToken(
                issuer: _issuer,
                audience: _audience,
                claims: claims,
                expires: expiration,
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
