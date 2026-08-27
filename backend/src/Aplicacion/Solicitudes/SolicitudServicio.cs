using HelpDesk.Aplicacion.Solicitudes.DTOs;
using HelpDesk.Dominio.Entidades;
using HelpDesk.Dominio.Excepciones;
using HelpDesk.Dominio.Reglas;

namespace HelpDesk.Aplicacion.Solicitudes;

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
            var errores = new Dictionary<string, string[]>();
            if (consulta.Page < 1)
                errores["page"] = new[] { "page debe ser mayor o igual a 1." };
            if (consulta.PageSize < 1 || consulta.PageSize > 100)
                errores["pageSize"] = new[] { "pageSize debe estar entre 1 y 100." };

            throw new ExcepcionNegocio(
                codigo: "PARAMETRO_INVALIDO",
                message: "Los parámetros de paginación están fuera de rango.",
                status: 400,
                detail: "page debe ser mayor o igual a 1 y pageSize debe estar entre 1 y 100.",
                errores: errores);
        }

        if (!OrdenamientoSolicitudes.EsValido(consulta.Sort))
        {
            throw new ExcepcionNegocio(
                codigo: "PARAMETRO_INVALIDO",
                message: "El parámetro sort no es válido.",
                status: 400,
                detail: $"Valores permitidos: {string.Join(", ", OrdenamientoSolicitudes.Default, OrdenamientoSolicitudes.Codigo, OrdenamientoSolicitudes.Prioridad, OrdenamientoSolicitudes.PrioridadDesc, OrdenamientoSolicitudes.FechaCreacion)}.",
                errores: new Dictionary<string, string[]>
                {
                    ["sort"] = new[] { "El parámetro sort no es válido." }
                });
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

    public async Task<SolicitudDto> ObtenerAsync(Guid id, Guid tenantId, RolUsuario rol, Guid usuarioId)
    {
        var solicitud = await ObtenerDeLaOrganizacionAsync(id, tenantId);
        PermisosSolicitud.Verificar(rol, AccionesSolicitud.Ver, esPropia: solicitud.SolicitanteId == usuarioId, solicitud.Estado);

        return ConstruirDetalle(solicitud, DateTime.UtcNow);
    }

    public async Task<SolicitudDto> CrearAsync(SolicitudRequest request, Guid tenantId, Guid solicitanteId)
    {
        var categoria = await ObtenerCategoriaDeLaOrganizacionAsync(request.CategoriaId!.Value, tenantId);

        var ahora = DateTime.UtcNow;
        var totalDelAnio = await _datos.ContarPorOrgYAnioAsync(tenantId, ahora.Year);

        var solicitud = new Solicitud
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            Codigo = GeneradorCodigoSolicitud.Generar(ahora.Year, totalDelAnio),
            Titulo = request.Titulo!,
            Descripcion = request.Descripcion!,
            Estado = EstadoSolicitud.Nueva,
            Prioridad = request.Prioridad!.Value,
            CategoriaId = categoria.Id,
            SolicitanteId = solicitanteId,
            AgenteId = null,
            FechaCreacion = ahora,
            FechaLimiteSla = CalculadorSla.Calcular(ahora, categoria.SlaHoras, request.Prioridad!.Value)
        };

        await _datos.GuardarAsync(solicitud);

        var creada = await _datos.BuscarPorIdAsync(solicitud.Id, tenantId);
        return ConstruirDetalle(creada!, ahora);
    }

    public async Task<SolicitudDto> ActualizarAsync(Guid id, SolicitudRequest request, Guid tenantId, RolUsuario rol, Guid usuarioId)
    {
        var solicitud = await ObtenerDeLaOrganizacionAsync(id, tenantId);
        PermisosSolicitud.Verificar(rol, AccionesSolicitud.Editar, esPropia: solicitud.SolicitanteId == usuarioId, solicitud.Estado);

        var categoria = await ObtenerCategoriaDeLaOrganizacionAsync(request.CategoriaId!.Value, tenantId);

        var cambiaSla = solicitud.Prioridad != request.Prioridad!.Value || solicitud.CategoriaId != categoria.Id;
        var estadoFinal = solicitud.Estado is EstadoSolicitud.Resuelta or EstadoSolicitud.Cerrada or EstadoSolicitud.Cancelada;

        solicitud.Titulo = request.Titulo!;
        solicitud.Descripcion = request.Descripcion!;
        solicitud.CategoriaId = categoria.Id;
        solicitud.Prioridad = request.Prioridad!.Value;

        if (cambiaSla && !estadoFinal)
        {
            solicitud.FechaLimiteSla = CalculadorSla.Recalcular(
                solicitud.FechaCreacion, categoria.SlaHoras, solicitud.Prioridad);
        }

        await _datos.GuardarAsync(solicitud);

        var actualizada = await _datos.BuscarPorIdAsync(solicitud.Id, tenantId);
        return ConstruirDetalle(actualizada!, DateTime.UtcNow);
    }

    public async Task<SolicitudDto> EjecutarTransicionAsync(
        Guid id, TransicionRequest request, Guid tenantId, RolUsuario rol, Guid usuarioId)
    {
        var solicitud = await ObtenerDeLaOrganizacionAsync(id, tenantId);
        var accion = request.Accion!;

        if (!AccionesSolicitud.Validas.Contains(accion))
        {
            throw new ExcepcionTransicionInvalida(solicitud.Estado, accion);
        }

        PermisosSolicitud.Verificar(rol, accion, esPropia: solicitud.SolicitanteId == usuarioId, solicitud.Estado);

        solicitud.Estado = MaquinaEstadosSolicitud.AplicarAccion(solicitud.Estado, accion);

        switch (accion)
        {
            case AccionesSolicitud.Asignar:
                var agente = request.AgenteId is null
                    ? null
                    : await _datos.BuscarAgenteAsync(request.AgenteId.Value, tenantId);

                if (agente is null || !agente.Activo || agente.Rol is not (RolUsuario.Agente or RolUsuario.Admin))
                {
                    throw new ExcepcionNegocio(
                        codigo: "AGENTE_INVALIDO",
                        message: "El agente indicado no es válido.",
                        status: 422,
                        detail: "El agente debe existir, estar activo, pertenecer a la organización y tener rol Agente o Admin.");
                }

                solicitud.AgenteId = agente.Id;
                break;

            case AccionesSolicitud.Resolver:
                if (string.IsNullOrWhiteSpace(request.Motivo) || request.Motivo.Trim().Length < 20)
                {
                    throw new ExcepcionNegocio(
                        codigo: "MOTIVO_REQUERIDO",
                        message: "El motivo de resolución es requerido.",
                        status: 422,
                        detail: "El motivo debe tener al menos 20 caracteres.");
                }

                solicitud.MotivoResolucion = request.Motivo.Trim();
                solicitud.FechaResolucion = DateTime.UtcNow;
                break;

            case AccionesSolicitud.Cancelar:
                if (string.IsNullOrWhiteSpace(request.Motivo) || request.Motivo.Trim().Length < 10)
                {
                    throw new ExcepcionNegocio(
                        codigo: "MOTIVO_REQUERIDO",
                        message: "El motivo de cancelación es requerido.",
                        status: 422,
                        detail: "El motivo debe tener al menos 10 caracteres.");
                }

                solicitud.MotivoCancelacion = request.Motivo.Trim();
                break;
        }

        await _datos.GuardarAsync(solicitud);

        var actualizada = await _datos.BuscarPorIdAsync(solicitud.Id, tenantId);
        return ConstruirDetalle(actualizada!, DateTime.UtcNow);
    }

    private async Task<Solicitud> ObtenerDeLaOrganizacionAsync(Guid id, Guid tenantId)
    {
        var solicitud = await _datos.BuscarPorIdAsync(id, tenantId);
        if (solicitud is null)
        {
            throw new ExcepcionNegocio(
                codigo: "RECURSO_NO_ENCONTRADO",
                message: "Recurso no encontrado.",
                status: 404);
        }

        return solicitud;
    }

    private async Task<Categoria> ObtenerCategoriaDeLaOrganizacionAsync(Guid id, Guid tenantId)
    {
        var categoria = await _datos.BuscarCategoriaAsync(id, tenantId);
        if (categoria is null)
        {
            throw new ExcepcionNegocio(
                codigo: "RECURSO_NO_ENCONTRADO",
                message: "Recurso no encontrado.",
                status: 404);
        }

        return categoria;
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

    private static SolicitudDto ConstruirDetalle(Solicitud s, DateTime ahora) => new()
    {
        Id = s.Id,
        Codigo = s.Codigo,
        Titulo = s.Titulo,
        Descripcion = s.Descripcion,
        Estado = s.Estado,
        Prioridad = s.Prioridad,
        Categoria = new CategoriaResumenDto { Id = s.Categoria.Id, Nombre = s.Categoria.Nombre },
        Solicitante = new UsuarioResumenDto { Id = s.Solicitante.Id, Nombre = s.Solicitante.Nombre },
        Agente = s.Agente is null ? null : new UsuarioResumenDto { Id = s.Agente.Id, Nombre = s.Agente.Nombre },
        FechaCreacion = s.FechaCreacion,
        FechaLimiteSla = s.FechaLimiteSla,
        FechaResolucion = s.FechaResolucion,
        MotivoResolucion = s.MotivoResolucion,
        MotivoCancelacion = s.MotivoCancelacion,
        Vencida = CalculadorSla.EstaVencida(s.FechaLimiteSla, s.Estado, ahora)
    };
}
