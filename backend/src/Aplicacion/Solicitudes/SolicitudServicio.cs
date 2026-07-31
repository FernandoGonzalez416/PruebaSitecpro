using MesaSitec.Aplicacion.Solicitudes.DTOs;
using MesaSitec.Dominio.Entidades;
using MesaSitec.Dominio.Excepciones;
using MesaSitec.Dominio.Reglas;

namespace MesaSitec.Aplicacion.Solicitudes;

public class SolicitudServicio : ISolicitudServicio
{
    private readonly ISolicitudDatos _datos;

    public SolicitudServicio(ISolicitudDatos datos)
    {
        _datos = datos;
    }

    public async Task<List<CategoriaDto>> ListarCategoriasAsync(Guid tenantId)
    {
        var categorias = await _datos.ListarCategoriasActivasAsync(tenantId);

        return categorias
            .Select(c => new CategoriaDto { Id = c.Id, Nombre = c.Nombre, SlaHoras = c.SlaHoras })
            .ToList();
    }

    public async Task<ListadoSolicitudesResponse> ListarAsync(
        ConsultaListadoSolicitudes consulta, Guid tenantId, RolUsuario rol, Guid usuarioId)
    {
        if (consulta.Page < 1 || consulta.PageSize < 1 || consulta.PageSize > 100)
        {
            throw new ExcepcionNegocio(
                codigo: "PARAMETRO_INVALIDO",
                message: "Los parámetros de paginación están fuera de rango.",
                status: 400,
                detail: "page debe ser mayor o igual a 1 y pageSize debe estar entre 1 y 100.");
        }

        if (!OrdenamientoSolicitudes.EsValido(consulta.Sort))
        {
            throw new ExcepcionNegocio(
                codigo: "PARAMETRO_INVALIDO",
                message: "El parámetro sort no es válido.",
                status: 400,
                detail: $"Valores permitidos: {string.Join(", ", OrdenamientoSolicitudes.Default, OrdenamientoSolicitudes.Codigo, OrdenamientoSolicitudes.Prioridad, OrdenamientoSolicitudes.PrioridadDesc, OrdenamientoSolicitudes.FechaCreacion)}.");
        }

        Guid? solicitanteId = rol == RolUsuario.Solicitante ? usuarioId : null;
        var ahora = DateTime.UtcNow;
        var resultado = await _datos.ListarAsync(consulta, tenantId, solicitanteId, ahora);

        var totalPaginas = resultado.Total == 0
            ? 0
            : (int)Math.Ceiling(resultado.Total / (double)consulta.PageSize);

        return new ListadoSolicitudesResponse
        {
            Items = resultado.Items.Select(s => ConstruirItem(s, ahora)).ToList(),
            Page = consulta.Page,
            PageSize = consulta.PageSize,
            Total = resultado.Total,
            TotalPaginas = totalPaginas
        };
    }

    private static SolicitudListaItemDto ConstruirItem(Solicitud s, DateTime ahora) => new()
    {
        Id = s.Id,
        Codigo = s.Codigo,
        Titulo = s.Titulo,
        Estado = s.Estado,
        Prioridad = s.Prioridad,
        Categoria = new CategoriaResumenDto { Id = s.Categoria.Id, Nombre = s.Categoria.Nombre },
        Agente = s.Agente is null ? null : new UsuarioResumenDto { Id = s.Agente.Id, Nombre = s.Agente.Nombre },
        FechaCreacion = s.FechaCreacion,
        FechaLimiteSla = s.FechaLimiteSla,
        Vencida = CalculadorSla.EstaVencida(s.FechaLimiteSla, s.Estado, ahora)
    };
}
