using System.ComponentModel.DataAnnotations;

namespace HelpDesk.Aplicacion.Solicitudes.DTOs;

public class TransicionRequest
{
    [Required(ErrorMessage = "La acción es obligatoria.")]
    public string? Accion { get; set; }

    public Guid? AgenteId { get; set; }

    public string? Motivo { get; set; }
}
