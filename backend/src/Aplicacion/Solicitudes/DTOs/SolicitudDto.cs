using MesaSitec.Dominio.Entidades;

namespace MesaSitec.Aplicacion.Solicitudes.DTOs;

public class SolicitudDto
{
    public Guid Id { get; set; }
    public string Codigo { get; set; } = null!;
    public string Titulo { get; set; } = null!;
    public string Descripcion { get; set; } = null!;
    public EstadoSolicitud Estado { get; set; }
    public Prioridad Prioridad { get; set; }
    public CategoriaResumenDto Categoria { get; set; } = null!;
    public UsuarioResumenDto Solicitante { get; set; } = null!;
    public UsuarioResumenDto? Agente { get; set; }
    public DateTime FechaCreacion { get; set; }
    public DateTime FechaLimiteSla { get; set; }
    public DateTime? FechaResolucion { get; set; }
    public string? MotivoResolucion { get; set; }
    public string? MotivoCancelacion { get; set; }
    public bool Vencida { get; set; }
}
