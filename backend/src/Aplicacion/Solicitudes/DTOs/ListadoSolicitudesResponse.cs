namespace MesaSitec.Aplicacion.Solicitudes.DTOs;

public class ListadoSolicitudesResponse
{
    public IReadOnlyList<SolicitudListaItemDto> Items { get; set; } = Array.Empty<SolicitudListaItemDto>();
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int Total { get; set; }
    public int TotalPaginas { get; set; }
}
