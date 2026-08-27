using HelpDesk.Aplicacion.Autenticacion.DTOs;

namespace HelpDesk.Aplicacion.Autenticacion;

public interface IAutenticacionServicio
{
    Task<LoginResponse> LoginAsync(LoginRequest request);
    Task<UsuarioDto> ObtenerUsuarioAsync(Guid usuarioId);
}
