using MesaSitec.Aplicacion.Solicitudes.DTOs;

namespace MesaSitec.Aplicacion.Solicitudes;

public class SolicitudServicio : ISolicitudServicio
{
    private readonly ISolicitudDatos _datos;

    public SolicitudServicio(ISolicitudDatos datos)
    {
        _datos = datos;
    }

    public async Task<List<CategoriaDto>> ListarCategoriasAsync(Guid tenantId)
    {
        var categorias = await _datos.ListarCategoriasActivasAsync(tenantId);

        return categorias
            .Select(c => new CategoriaDto { Id = c.Id, Nombre = c.Nombre, SlaHoras = c.SlaHoras })
            .ToList();
    }
}
