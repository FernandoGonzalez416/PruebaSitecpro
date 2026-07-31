using System.ComponentModel.DataAnnotations;
using MesaSitec.Dominio.Entidades;

namespace MesaSitec.Aplicacion.Solicitudes.DTOs;

public class SolicitudRequest
{
    [Required(ErrorMessage = "El título es obligatorio.")]
    [MinLength(5, ErrorMessage = "El título debe tener al menos 5 caracteres.")]
    [MaxLength(120, ErrorMessage = "El título debe tener como máximo 120 caracteres.")]
    public string? Titulo { get; set; }

    [Required(ErrorMessage = "La descripción es obligatoria.")]
    [MinLength(10, ErrorMessage = "La descripción debe tener al menos 10 caracteres.")]
    [MaxLength(4000, ErrorMessage = "La descripción debe tener como máximo 4000 caracteres.")]
    public string? Descripcion { get; set; }

    [Required(ErrorMessage = "La categoría es obligatoria.")]
    public Guid? CategoriaId { get; set; }

    [Required(ErrorMessage = "La prioridad es obligatoria.")]
    public Prioridad? Prioridad { get; set; }
}
