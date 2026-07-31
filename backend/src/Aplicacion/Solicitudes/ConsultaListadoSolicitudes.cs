using MesaSitec.Dominio.Entidades;

namespace MesaSitec.Aplicacion.Solicitudes;

public record ConsultaListadoSolicitudes(
    EstadoSolicitud? Estado,
    Prioridad? Prioridad,
    Guid? CategoriaId,
    Guid? AgenteId,
    string? Q,
    bool Vencidas,
    int Page,
    int PageSize,
    string Sort);
