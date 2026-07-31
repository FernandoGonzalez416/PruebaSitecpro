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
