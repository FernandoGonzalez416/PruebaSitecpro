using HelpDesk.Aplicacion.Autenticacion.DTOs;
using HelpDesk.Dominio.Entidades;
using HelpDesk.Dominio.Excepciones;

namespace HelpDesk.Aplicacion.Autenticacion;

public class AutenticacionServicio : IAutenticacionServicio
{
    private readonly IAutenticacionDatos _datos;
    private readonly IVerificadorPassword _verificador;
    private readonly IGeneradorTokens _generador;

    public AutenticacionServicio(
        IAutenticacionDatos datos,
        IVerificadorPassword verificador,
        IGeneradorTokens generador)
    {
        _datos = datos;
        _verificador = verificador;
        _generador = generador;
    }

    public async Task<LoginResponse> LoginAsync(LoginRequest request)
    {
        var usuario = await _datos.BuscarPorEmailAsync(request.Email);
        if (usuario is null || !usuario.Activo)
        {
            throw new ExcepcionNoAutenticado();
        }

        if (!_verificador.Verificar(request.Password, usuario.PasswordHash))
        {
            throw new ExcepcionNoAutenticado();
        }

        var token = _generador.GenerarToken(usuario.Id, usuario.TenantId, usuario.Rol, usuario.Email);
        var usuarioDto = await ConstruirUsuarioDtoAsync(usuario);

        return new LoginResponse
        {
            AccessToken = token,
            ExpiraEn = 28800,
            Usuario = usuarioDto
        };
    }

    public async Task<UsuarioDto> ObtenerUsuarioAsync(Guid usuarioId)
    {
        var usuario = await _datos.BuscarPorIdAsync(usuarioId);
        if (usuario is null)
        {
            throw new ExcepcionNegocio(
                codigo: "RECURSO_NO_ENCONTRADO",
                message: "Recurso no encontrado.",
                status: 404);
        }

        return await ConstruirUsuarioDtoAsync(usuario);
    }

    private async Task<UsuarioDto> ConstruirUsuarioDtoAsync(Usuario usuario)
    {
        var tenant = await _datos.BuscarTenantAsync(usuario.TenantId);
        if (tenant is null)
        {
            throw new ExcepcionNegocio(
                codigo: "RECURSO_NO_ENCONTRADO",
                message: "Recurso no encontrado.",
                status: 404);
        }

        return new UsuarioDto
        {
            Id = usuario.Id,
            Nombre = usuario.Nombre,
            Email = usuario.Email,
            Rol = usuario.Rol.ToString(),
            TenantId = usuario.TenantId,
            TenantNombre = tenant.Nombre
        };
    }
}
