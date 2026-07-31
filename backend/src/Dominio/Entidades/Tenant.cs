namespace MesaSitec.Dominio.Entidades;

public class Tenant
{
    public Guid Id { get; set; }
    public string Nombre { get; set; } = null!;
    public bool Activo { get; set; }
}
