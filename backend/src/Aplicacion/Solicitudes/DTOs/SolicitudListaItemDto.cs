using MesaSitec.Dominio.Entidades;

namespace MesaSitec.Aplicacion.Solicitudes.DTOs;

public class SolicitudListaItemDto
{
    public Guid Id { get; set; }
    public string Codigo { get; set; } = null!;
    public string Titulo { get; set; } = null!;
    public EstadoSolicitud Estado { get; set; }
    public Prioridad Prioridad { get; set; }
    public CategoriaResumenDto Categoria { get; set; } = null!;
    public UsuarioResumenDto? Agente { get; set; }
    public DateTime FechaCreacion { get; set; }
    public DateTime FechaLimiteSla { get; set; }
    public bool Vencida { get; set; }
}
