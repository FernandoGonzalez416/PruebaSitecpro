using MesaSitec.Aplicacion.Autenticacion.DTOs;

namespace MesaSitec.Aplicacion.Autenticacion;

public interface IAutenticacionServicio
{
    Task<LoginResponse> LoginAsync(LoginRequest request);
    Task<UsuarioDto> ObtenerUsuarioAsync(Guid usuarioId);
}
