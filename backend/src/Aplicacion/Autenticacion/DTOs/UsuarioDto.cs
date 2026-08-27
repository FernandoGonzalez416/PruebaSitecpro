namespace HelpDesk.Aplicacion.Autenticacion.DTOs;

public class UsuarioDto
{
    public Guid Id { get; set; }
    public string Nombre { get; set; } = null!;
    public string Email { get; set; } = null!;
    public string Rol { get; set; } = null!;
    public Guid TenantId { get; set; }
    public string TenantNombre { get; set; } = null!;
}
