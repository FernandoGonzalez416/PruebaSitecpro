using HelpDesk.Dominio.Entidades;
using HelpDesk.Dominio.Excepciones;

namespace HelpDesk.Dominio.Reglas;

public static class MaquinaEstadosSolicitud
{
    private static readonly Dictionary<(EstadoSolicitud Estado, string Accion), EstadoSolicitud> Transiciones =
        new()
        {
            [(EstadoSolicitud.Nueva, AccionesSolicitud.Asignar)] = EstadoSolicitud.Asignada,
            [(EstadoSolicitud.Nueva, AccionesSolicitud.Cancelar)] = EstadoSolicitud.Cancelada,
            [(EstadoSolicitud.Asignada, AccionesSolicitud.Iniciar)] = EstadoSolicitud.EnProceso,
            [(EstadoSolicitud.Asignada, AccionesSolicitud.Asignar)] = EstadoSolicitud.Asignada,
            [(EstadoSolicitud.Asignada, AccionesSolicitud.Cancelar)] = EstadoSolicitud.Cancelada,
            [(EstadoSolicitud.EnProceso, AccionesSolicitud.Resolver)] = EstadoSolicitud.Resuelta,
            [(EstadoSolicitud.EnProceso, AccionesSolicitud.Asignar)] = EstadoSolicitud.Asignada,
            [(EstadoSolicitud.EnProceso, AccionesSolicitud.Cancelar)] = EstadoSolicitud.Cancelada,
            [(EstadoSolicitud.Resuelta, AccionesSolicitud.Cerrar)] = EstadoSolicitud.Cerrada,
            [(EstadoSolicitud.Resuelta, AccionesSolicitud.Reabrir)] = EstadoSolicitud.EnProceso
        };

    public static EstadoSolicitud AplicarAccion(EstadoSolicitud estadoActual, string accion)
    {
        if (Transiciones.TryGetValue((estadoActual, accion), out var estadoDestino))
        {
            return estadoDestino;
        }

        throw new ExcepcionTransicionInvalida(estadoActual, accion);
    }
}
