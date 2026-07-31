using MesaSitec.Dominio.Entidades;

namespace MesaSitec.Aplicacion.Solicitudes;

public interface ISolicitudDatos
{
    Task<List<Categoria>> ListarCategoriasActivasAsync(Guid tenantId);
    Task<Categoria?> BuscarCategoriaAsync(Guid id, Guid tenantId);
    Task<Usuario?> BuscarAgenteAsync(Guid id, Guid tenantId);
    Task<int> ContarPorOrgYAnioAsync(Guid tenantId, int anio);
    Task<ListadoSolicitudesResultado> ListarAsync(ConsultaListadoSolicitudes consulta, Guid tenantId, Guid? solicitanteId, DateTime ahora);
    Task<Solicitud?> BuscarPorIdAsync(Guid id, Guid tenantId);
    Task GuardarAsync(Solicitud solicitud);
}
