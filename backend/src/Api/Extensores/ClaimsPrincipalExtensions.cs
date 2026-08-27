using System.Security.Claims;
using HelpDesk.Dominio.Entidades;
using HelpDesk.Dominio.Excepciones;

namespace HelpDesk.Api.Extensores;

public static class ClaimsPrincipalExtensions
{
    public static Guid UsuarioId(this ClaimsPrincipal user) => ObtenerGuid(user, "sub");

    public static Guid TenantId(this ClaimsPrincipal user) => ObtenerGuid(user, "tenantId");

    public static RolUsuario Rol(this ClaimsPrincipal user)
    {
        var rol = user.FindFirstValue("rol");
        if (Enum.TryParse<RolUsuario>(rol, out var resultado))
        {
            return resultado;
        }

        throw new ExcepcionNoAutenticado("Token sin claim rol válido.");
    }

    private static Guid ObtenerGuid(ClaimsPrincipal user, string tipo)
    {
        var valor = user.FindFirstValue(tipo);
        if (valor is not null && Guid.TryParse(valor, out var id))
        {
            return id;
        }

        throw new ExcepcionNoAutenticado($"Token sin claim {tipo} válido.");
    }
}
