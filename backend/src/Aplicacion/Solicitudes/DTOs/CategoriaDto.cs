namespace MesaSitec.Aplicacion.Solicitudes.DTOs;

public class CategoriaDto
{
    public Guid Id { get; set; }
    public string Nombre { get; set; } = null!;
    public int SlaHoras { get; set; }
}
