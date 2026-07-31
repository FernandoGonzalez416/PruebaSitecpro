namespace MesaSitec.Dominio.Entidades;

public class Solicitud
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
    public string Codigo { get; set; } = null!;
    public string Titulo { get; set; } = null!;
    public string Descripcion { get; set; } = null!;
    public EstadoSolicitud Estado { get; set; }
    public Prioridad Prioridad { get; set; }
    public Guid CategoriaId { get; set; }
    public Guid SolicitanteId { get; set; }
    public Guid? AgenteId { get; set; }
    public DateTime FechaCreacion { get; set; }
    public DateTime FechaLimiteSla { get; set; }
    public DateTime? FechaResolucion { get; set; }
    public string? MotivoResolucion { get; set; }
    public string? MotivoCancelacion { get; set; }

    public Categoria Categoria { get; set; } = null!;
    public Usuario Solicitante { get; set; } = null!;
    public Usuario? Agente { get; set; }
}
