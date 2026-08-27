using HelpDesk.Dominio.Entidades;
using HelpDesk.Dominio.Excepciones;

namespace HelpDesk.Dominio.Reglas;

public static class PermisosSolicitud
{
    public static void Verificar(RolUsuario rol, string accion, bool esPropia, EstadoSolicitud estado)
    {
        if (!EsPermitido(rol, accion, esPropia, estado))
        {
            throw new ExcepcionOperacionNoPermitida(rol, accion);
        }
    }

    public static bool EsPermitido(RolUsuario rol, string accion, bool esPropia, EstadoSolicitud estado)
    {
        return accion switch
        {
            AccionesSolicitud.Asignar or AccionesSolicitud.Iniciar or AccionesSolicitud.Resolver
                or AccionesSolicitud.Reabrir =>
                rol is RolUsuario.Admin or RolUsuario.Agente,

            AccionesSolicitud.Cerrar =>
                rol != RolUsuario.Solicitante || esPropia,

            AccionesSolicitud.Cancelar =>
                rol == RolUsuario.Admin,

            AccionesSolicitud.Editar =>
                rol != RolUsuario.Solicitante || (esPropia && estado == EstadoSolicitud.Nueva),

            AccionesSolicitud.Ver =>
                rol != RolUsuario.Solicitante || esPropia,

            _ => false
        };
    }
}
