using MesaSitec.Aplicacion.Solicitudes.DTOs;

namespace MesaSitec.Aplicacion.Solicitudes;

public interface ISolicitudServicio
{
    Task<List<CategoriaDto>> ListarCategoriasAsync(Guid tenantId);
}
