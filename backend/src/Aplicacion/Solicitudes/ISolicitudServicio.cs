using MesaSitec.Aplicacion.Solicitudes.DTOs;
using MesaSitec.Dominio.Entidades;

namespace MesaSitec.Aplicacion.Solicitudes;

public interface ISolicitudServicio
{
    Task<List<CategoriaDto>> ListarCategoriasAsync(Guid tenantId);
    Task<ListadoSolicitudesResponse> ListarAsync(ConsultaListadoSolicitudes consulta, Guid tenantId, RolUsuario rol, Guid usuarioId);
    Task<SolicitudDto> ObtenerAsync(Guid id, Guid tenantId, RolUsuario rol, Guid usuarioId);
    Task<SolicitudDto> CrearAsync(SolicitudRequest request, Guid tenantId, Guid solicitanteId);
}
