namespace HelpDesk.Dominio.Reglas;

public static class GeneradorCodigoSolicitud
{
    public static string Generar(int anio, int totalEnAnio) =>
        $"SOL-{anio}-{totalEnAnio + 1:D5}";
}
