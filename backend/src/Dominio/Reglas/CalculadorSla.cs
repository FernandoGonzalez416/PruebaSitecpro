using HelpDesk.Dominio.Entidades;

namespace HelpDesk.Dominio.Reglas;

public static class CalculadorSla
{
    private static readonly Dictionary<Prioridad, double> Factores = new()
    {
        [Prioridad.Critica] = 0.5,
        [Prioridad.Alta] = 0.75,
        [Prioridad.Media] = 1.0,
        [Prioridad.Baja] = 2.0
    };

    public static DateTime Calcular(DateTime fechaCreacion, int slaHoras, Prioridad prioridad)
    {
        return fechaCreacion.AddHours(slaHoras * Factores[prioridad]);
    }

    public static DateTime Recalcular(DateTime fechaCreacion, int slaHoras, Prioridad prioridad)
    {
        return Calcular(fechaCreacion, slaHoras, prioridad);
    }

    public static bool EstaVencida(DateTime fechaLimiteSla, EstadoSolicitud estado, DateTime ahora)
    {
        return !EsEstadoFinal(estado) && ahora > fechaLimiteSla;
    }

    private static bool EsEstadoFinal(EstadoSolicitud estado)
    {
        return estado is EstadoSolicitud.Resuelta or EstadoSolicitud.Cerrada or EstadoSolicitud.Cancelada;
    }
}
