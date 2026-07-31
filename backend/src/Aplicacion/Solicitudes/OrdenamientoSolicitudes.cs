namespace MesaSitec.Aplicacion.Solicitudes;

public static class OrdenamientoSolicitudes
{
    public const string FechaCreacion = "fechaCreacion";
    public const string FechaCreacionDesc = "-fechaCreacion";
    public const string Prioridad = "prioridad";
    public const string PrioridadDesc = "-prioridad";
    public const string Codigo = "codigo";
    public const string Default = FechaCreacionDesc;

    public static bool EsValido(string sort) =>
        sort is FechaCreacion or FechaCreacionDesc or Prioridad or PrioridadDesc or Codigo;
}
