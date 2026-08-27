using HelpDesk.Dominio.Entidades;

namespace HelpDesk.Aplicacion.Autenticacion;

public interface IGeneradorTokens
{
    string GenerarToken(Guid usuarioId, Guid tenantId, RolUsuario rol, string email);
}
