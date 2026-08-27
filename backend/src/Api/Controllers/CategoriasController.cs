using HelpDesk.Aplicacion.Solicitudes;
using HelpDesk.Aplicacion.Solicitudes.DTOs;
using HelpDesk.Api.Extensores;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HelpDesk.Api.Controllers;

[ApiController]
[Route("api/v1")]
[Authorize]
public class CategoriasController : ControllerBase
{
    private readonly ISolicitudServicio _servicio;

    public CategoriasController(ISolicitudServicio servicio)
    {
        _servicio = servicio;
    }

    [HttpGet("categorias")]
    public async Task<ActionResult<List<CategoriaDto>>> Listar() =>
        Ok(await _servicio.ListarCategoriasAsync(User.TenantId()));
}
