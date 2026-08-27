namespace HelpDesk.Dominio.Reglas;

public static class AccionesSolicitud
{
    public const string Asignar = "asignar";
    public const string Iniciar = "iniciar";
    public const string Resolver = "resolver";
    public const string Cerrar = "cerrar";
    public const string Reabrir = "reabrir";
    public const string Cancelar = "cancelar";
    public const string Ver = "ver";
    public const string Editar = "editar";

    public static readonly string[] Validas =
    {
        Asignar, Iniciar, Resolver, Cerrar, Reabrir, Cancelar
    };
}
