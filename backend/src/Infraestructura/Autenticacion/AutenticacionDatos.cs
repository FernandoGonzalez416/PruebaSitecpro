using HelpDesk.Aplicacion.Autenticacion;
using HelpDesk.Dominio.Entidades;
using HelpDesk.Infraestructura.Data;
using Microsoft.EntityFrameworkCore;

namespace HelpDesk.Infraestructura.Autenticacion;

public class AutenticacionDatos : IAutenticacionDatos
{
    private readonly HelpDeskDbContext _db;

    public AutenticacionDatos(HelpDeskDbContext db)
    {
        _db = db;
    }

    public Task<Usuario?> BuscarPorEmailAsync(string email) =>
        _db.Usuarios.AsNoTracking()
            .FirstOrDefaultAsync(u => u.Email.ToLower() == email.ToLowerInvariant());

    public Task<Usuario?> BuscarPorIdAsync(Guid id) =>
        _db.Usuarios.AsNoTracking().FirstOrDefaultAsync(u => u.Id == id);

    public Task<Tenant?> BuscarTenantAsync(Guid id) =>
        _db.Tenants.AsNoTracking().FirstOrDefaultAsync(t => t.Id == id);
}
