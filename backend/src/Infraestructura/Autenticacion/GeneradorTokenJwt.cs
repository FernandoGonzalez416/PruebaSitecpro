using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using HelpDesk.Aplicacion.Autenticacion;
using HelpDesk.Dominio.Entidades;
using Microsoft.IdentityModel.Tokens;

namespace HelpDesk.Infraestructura.Autenticacion;

public class GeneradorTokenJwt : IGeneradorTokens
{
    private readonly string _secret;
    private readonly string _issuer;
    private readonly string _audience;

    public GeneradorTokenJwt(string secret, string issuer, string audience)
    {
        if (string.IsNullOrWhiteSpace(secret) || secret.Length < 32)
        {
            throw new ArgumentException("JWT_SECRET debe tener al menos 32 caracteres.", nameof(secret));
        }

        _secret = secret;
        _issuer = issuer;
        _audience = audience;
    }

    public string GenerarToken(Guid usuarioId, Guid tenantId, RolUsuario rol, string email)
    {
        var clave = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_secret));
        var credenciales = new SigningCredentials(clave, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim("sub", usuarioId.ToString()),
            new Claim("tenantId", tenantId.ToString()),
            new Claim("rol", rol.ToString()),
            new Claim("email", email)
        };

        var token = new JwtSecurityToken(
            issuer: _issuer,
            audience: _audience,
            claims: claims,
            expires: DateTime.UtcNow.AddHours(8),
            signingCredentials: credenciales);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
