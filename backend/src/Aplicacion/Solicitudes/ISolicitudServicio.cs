using MesaSitec.Aplicacion.Solicitudes.DTOs;
using MesaSitec.Dominio.Entidades;

namespace MesaSitec.Aplicacion.Solicitudes;

public interface ISolicitudServicio
{
    Task<List<CategoriaDto>> ListarCategoriasAsync(Guid tenantId);
    Task<ListadoSolicitudesResponse> ListarAsync(ConsultaListadoSolicitudes consulta, Guid tenantId, RolUsuario rol, Guid usuarioId);
}
