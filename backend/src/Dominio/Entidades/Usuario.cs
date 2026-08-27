namespace HelpDesk.Dominio.Entidades;

public class Usuario
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
    public string Email { get; set; } = null!;
    public string PasswordHash { get; set; } = null!;
    public string Nombre { get; set; } = null!;
    public RolUsuario Rol { get; set; }
    public bool Activo { get; set; }
}
