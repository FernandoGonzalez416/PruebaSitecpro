namespace MesaSitec.Dominio.Entidades;

public enum RolUsuario
{
    Admin,
    Agente,
    Solicitante
}

public enum Prioridad
{
    Critica,
    Alta,
    Media,
    Baja
}

public enum EstadoSolicitud
{
    Nueva,
    Asignada,
    EnProceso,
    Resuelta,
    Cerrada,
    Cancelada
}
