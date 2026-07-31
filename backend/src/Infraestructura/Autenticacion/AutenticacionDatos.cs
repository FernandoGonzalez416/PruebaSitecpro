using MesaSitec.Aplicacion.Autenticacion;
using MesaSitec.Dominio.Entidades;
using MesaSitec.Infraestructura.Data;
using Microsoft.EntityFrameworkCore;

namespace MesaSitec.Infraestructura.Autenticacion;

public class AutenticacionDatos : IAutenticacionDatos
{
    private readonly MesaSitecDbContext _db;

    public AutenticacionDatos(MesaSitecDbContext db)
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
