namespace HelpDesk.Infraestructura.Data.Semilla;

public static class SeedIds
{
    public static readonly Guid TenantNorte = Guid.Parse("11111111-1111-1111-1111-111111111111");
    public static readonly Guid TenantSur = Guid.Parse("22222222-2222-2222-2222-222222222222");

    public static readonly Guid UsuarioAdminNorte = Guid.Parse("10000000-0000-0000-0000-000000000001");
    public static readonly Guid UsuarioAgente1Norte = Guid.Parse("10000000-0000-0000-0000-000000000002");
    public static readonly Guid UsuarioAgente2Norte = Guid.Parse("10000000-0000-0000-0000-000000000003");
    public static readonly Guid UsuarioUser1Norte = Guid.Parse("10000000-0000-0000-0000-000000000004");
    public static readonly Guid UsuarioUser2Norte = Guid.Parse("10000000-0000-0000-0000-000000000005");
    public static readonly Guid UsuarioAdminSur = Guid.Parse("20000000-0000-0000-0000-000000000001");
    public static readonly Guid UsuarioUser1Sur = Guid.Parse("20000000-0000-0000-0000-000000000002");

    public static readonly Guid CategoriaIncidenteNorte = Guid.Parse("30000000-0000-0000-0000-000000000001");
    public static readonly Guid CategoriaRequerimientoNorte = Guid.Parse("30000000-0000-0000-0000-000000000002");
    public static readonly Guid CategoriaConsultaNorte = Guid.Parse("30000000-0000-0000-0000-000000000003");
    public static readonly Guid CategoriaFallaCriticaNorte = Guid.Parse("30000000-0000-0000-0000-000000000004");
    public static readonly Guid CategoriaIncidenteSur = Guid.Parse("40000000-0000-0000-0000-000000000001");
    public static readonly Guid CategoriaRequerimientoSur = Guid.Parse("40000000-0000-0000-0000-000000000002");
    public static readonly Guid CategoriaConsultaSur = Guid.Parse("40000000-0000-0000-0000-000000000003");
    public static readonly Guid CategoriaFallaCriticaSur = Guid.Parse("40000000-0000-0000-0000-000000000004");
}
