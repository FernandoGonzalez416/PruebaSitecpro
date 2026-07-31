using MesaSitec.Aplicacion.Solicitudes;
using MesaSitec.Dominio.Entidades;
using MesaSitec.Infraestructura.Data;
using Microsoft.EntityFrameworkCore;

namespace MesaSitec.Infraestructura.Solicitudes;

public class SolicitudDatos : ISolicitudDatos
{
    private readonly MesaSitecDbContext _db;

    public SolicitudDatos(MesaSitecDbContext db)
    {
        _db = db;
    }

    public Task<List<Categoria>> ListarCategoriasActivasAsync(Guid tenantId) =>
        _db.Categorias.AsNoTracking()
            .Where(c => c.TenantId == tenantId && c.Activo)
            .OrderBy(c => c.Nombre)
            .ToListAsync();
}
