using MesaSitec.Aplicacion.Solicitudes;
using MesaSitec.Aplicacion.Solicitudes.DTOs;
using MesaSitec.Api.Extensores;
using MesaSitec.Dominio.Entidades;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MesaSitec.Api.Controllers;

[ApiController]
[Route("api/v1")]
[Authorize]
public class SolicitudesController : ControllerBase
{
    private readonly ISolicitudServicio _servicio;

    public SolicitudesController(ISolicitudServicio servicio)
    {
        _servicio = servicio;
    }

    [HttpGet("solicitudes")]
    public async Task<ActionResult<ListadoSolicitudesResponse>> Listar(
        [FromQuery] EstadoSolicitud? estado,
        [FromQuery] Prioridad? prioridad,
        [FromQuery] Guid? categoriaId,
        [FromQuery] Guid? agenteId,
        [FromQuery] string? q,
        [FromQuery] bool? vencidas,
        [FromQuery] int? page,
        [FromQuery] int? pageSize,
        [FromQuery] string? sort)
    {
        var consulta = new ConsultaListadoSolicitudes(
            Estado: estado,
            Prioridad: prioridad,
            CategoriaId: categoriaId,
            AgenteId: agenteId,
            Q: q,
            Vencidas: vencidas ?? false,
            Page: page ?? 1,
            PageSize: pageSize ?? 20,
            Sort: sort ?? OrdenamientoSolicitudes.Default);

        return Ok(await _servicio.ListarAsync(consulta, User.TenantId(), User.Rol(), User.UsuarioId()));
    }

    [HttpGet("solicitudes/{id:guid}")]
    public async Task<ActionResult<SolicitudDto>> Obtener(Guid id) =>
        Ok(await _servicio.ObtenerAsync(id, User.TenantId(), User.Rol(), User.UsuarioId()));

    [HttpPost("solicitudes")]
    public async Task<ActionResult<SolicitudDto>> Crear([FromBody] SolicitudRequest request)
    {
        var solicitud = await _servicio.CrearAsync(request, User.TenantId(), User.UsuarioId());

        return CreatedAtAction(nameof(Obtener), new { id = solicitud.Id }, solicitud);
    }

    [HttpPut("solicitudes/{id:guid}")]
    public async Task<ActionResult<SolicitudDto>> Actualizar(Guid id, [FromBody] SolicitudRequest request) =>
        Ok(await _servicio.ActualizarAsync(id, request, User.TenantId(), User.Rol(), User.UsuarioId()));

    [HttpPost("solicitudes/{id:guid}/transiciones")]
    public async Task<ActionResult<SolicitudDto>> Transicionar(Guid id, [FromBody] TransicionRequest request) =>
        Ok(await _servicio.EjecutarTransicionAsync(id, request, User.TenantId(), User.Rol(), User.UsuarioId()));
}
