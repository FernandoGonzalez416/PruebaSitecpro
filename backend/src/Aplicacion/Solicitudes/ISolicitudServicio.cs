using HelpDesk.Aplicacion.Solicitudes.DTOs;
using HelpDesk.Dominio.Entidades;

namespace HelpDesk.Aplicacion.Solicitudes;

public interface ISolicitudServicio
{
    Task<List<CategoriaDto>> ListarCategoriasAsync(Guid tenantId);
    Task<ListadoSolicitudesResponse> ListarAsync(ConsultaListadoSolicitudes consulta, Guid tenantId, RolUsuario rol, Guid usuarioId);
    Task<SolicitudDto> ObtenerAsync(Guid id, Guid tenantId, RolUsuario rol, Guid usuarioId);
    Task<SolicitudDto> CrearAsync(SolicitudRequest request, Guid tenantId, Guid solicitanteId);
    Task<SolicitudDto> ActualizarAsync(Guid id, SolicitudRequest request, Guid tenantId, RolUsuario rol, Guid usuarioId);
    Task<SolicitudDto> EjecutarTransicionAsync(Guid id, TransicionRequest request, Guid tenantId, RolUsuario rol, Guid usuarioId);
}
