using MesaSitec.Aplicacion.Solicitudes;
using MesaSitec.Aplicacion.Solicitudes.DTOs;
using MesaSitec.Api.Extensores;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MesaSitec.Api.Controllers;

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
