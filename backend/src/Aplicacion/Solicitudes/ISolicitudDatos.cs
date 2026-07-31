using MesaSitec.Dominio.Entidades;

namespace MesaSitec.Aplicacion.Solicitudes;

public interface ISolicitudDatos
{
    Task<List<Categoria>> ListarCategoriasActivasAsync(Guid tenantId);
}
