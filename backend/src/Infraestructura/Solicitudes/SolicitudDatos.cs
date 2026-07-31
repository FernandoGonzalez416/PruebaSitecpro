using System.Linq.Expressions;
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

    public Task<Categoria?> BuscarCategoriaAsync(Guid id, Guid tenantId) =>
        _db.Categorias.AsNoTracking()
            .FirstOrDefaultAsync(c => c.Id == id && c.TenantId == tenantId);

    public Task<int> ContarPorOrgYAnioAsync(Guid tenantId, int anio)
    {
        var inicio = new DateTime(anio, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        var fin = inicio.AddYears(1);

        return _db.Solicitudes.CountAsync(s =>
            s.TenantId == tenantId && s.FechaCreacion >= inicio && s.FechaCreacion < fin);
    }

    public async Task<ListadoSolicitudesResultado> ListarAsync(
        ConsultaListadoSolicitudes consulta, Guid tenantId, Guid? solicitanteId, DateTime ahora)
    {
        var query = _db.Solicitudes.AsNoTracking()
            .Where(s => s.TenantId == tenantId);

        if (solicitanteId is not null)
        {
            query = query.Where(s => s.SolicitanteId == solicitanteId);
        }

        if (consulta.Estado is not null)
        {
            query = query.Where(s => s.Estado == consulta.Estado);
        }

        if (consulta.Prioridad is not null)
        {
            query = query.Where(s => s.Prioridad == consulta.Prioridad);
        }

        if (consulta.CategoriaId is not null)
        {
            query = query.Where(s => s.CategoriaId == consulta.CategoriaId);
        }

        if (consulta.AgenteId is not null)
        {
            query = query.Where(s => s.AgenteId == consulta.AgenteId);
        }

        if (!string.IsNullOrWhiteSpace(consulta.Q))
        {
            var q = consulta.Q.Trim().ToLower();
            query = query.Where(s =>
                s.Titulo.ToLower().Contains(q)
                || s.Descripcion.ToLower().Contains(q)
                || s.Codigo.ToLower().Contains(q));
        }

        if (consulta.Vencidas)
        {
            query = query.Where(s =>
                s.Estado != EstadoSolicitud.Resuelta
                && s.Estado != EstadoSolicitud.Cerrada
                && s.Estado != EstadoSolicitud.Cancelada
                && s.FechaLimiteSla < ahora);
        }

        var total = await query.CountAsync();

        var items = await Ordenar(query, consulta.Sort)
            .Skip((consulta.Page - 1) * consulta.PageSize)
            .Take(consulta.PageSize)
            .Include(s => s.Categoria)
            .Include(s => s.Agente)
            .ToListAsync();

        return new ListadoSolicitudesResultado(items, total);
    }

    public Task<Solicitud?> BuscarPorIdAsync(Guid id, Guid tenantId) =>
        _db.Solicitudes
            .Include(s => s.Categoria)
            .Include(s => s.Solicitante)
            .Include(s => s.Agente)
            .FirstOrDefaultAsync(s => s.Id == id && s.TenantId == tenantId);

    public async Task GuardarAsync(Solicitud solicitud)
    {
        if (_db.Entry(solicitud).State == EntityState.Detached)
        {
            _db.Solicitudes.Add(solicitud);
        }

        await _db.SaveChangesAsync();
    }

    private static IOrderedQueryable<Solicitud> Ordenar(IQueryable<Solicitud> query, string sort) => sort switch
    {
        OrdenamientoSolicitudes.FechaCreacion => query.OrderBy(s => s.FechaCreacion),
        OrdenamientoSolicitudes.Prioridad => query.OrderBy(IndicePrioridad),
        OrdenamientoSolicitudes.PrioridadDesc => query.OrderByDescending(IndicePrioridad),
        OrdenamientoSolicitudes.Codigo => query.OrderBy(s => s.Codigo),
        _ => query.OrderByDescending(s => s.FechaCreacion)
    };

    private static readonly Expression<Func<Solicitud, int>> IndicePrioridad =
        s => s.Prioridad == Prioridad.Critica ? 0
            : s.Prioridad == Prioridad.Alta ? 1
            : s.Prioridad == Prioridad.Media ? 2
            : 3;
}
