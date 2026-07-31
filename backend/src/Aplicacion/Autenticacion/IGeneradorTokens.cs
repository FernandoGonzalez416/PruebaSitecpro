using MesaSitec.Dominio.Entidades;

namespace MesaSitec.Aplicacion.Autenticacion;

public interface IGeneradorTokens
{
    string GenerarToken(Guid usuarioId, Guid tenantId, RolUsuario rol, string email);
}
