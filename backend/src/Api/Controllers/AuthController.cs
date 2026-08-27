using System.Security.Claims;
using HelpDesk.Aplicacion.Autenticacion;
using HelpDesk.Aplicacion.Autenticacion.DTOs;
using HelpDesk.Dominio.Excepciones;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HelpDesk.Api.Controllers;

[ApiController]
[Route("api/v1")]
public class AuthController : ControllerBase
{
    private readonly IAutenticacionServicio _servicio;

    public AuthController(IAutenticacionServicio servicio)
    {
        _servicio = servicio;
    }

    [AllowAnonymous]
    [HttpPost("auth/login")]
    public async Task<ActionResult<LoginResponse>> Login([FromBody] LoginRequest request) =>
        Ok(await _servicio.LoginAsync(request));

    [Authorize]
    [HttpGet("me")]
    public async Task<ActionResult<UsuarioDto>> Me()
    {
        var usuarioId = User.FindFirstValue("sub");
        if (!Guid.TryParse(usuarioId, out var id))
        {
            throw new ExcepcionNegocio(
                codigo: "NO_AUTENTICADO",
                message: "Token sin claim sub válido.",
                status: 401);
        }

        return Ok(await _servicio.ObtenerUsuarioAsync(id));
    }

    [AllowAnonymous]
    [HttpGet("health")]
    public ActionResult<object> Health() => Ok(new { estado = "ok" });
}
