using HelpDesk.Dominio.Entidades;

namespace HelpDesk.Aplicacion.Autenticacion;

public interface IAutenticacionDatos
{
    Task<Usuario?> BuscarPorEmailAsync(string email);
    Task<Usuario?> BuscarPorIdAsync(Guid id);
    Task<Tenant?> BuscarTenantAsync(Guid id);
}
