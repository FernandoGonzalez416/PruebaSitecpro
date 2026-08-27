using HelpDesk.Dominio.Entidades;

namespace HelpDesk.Aplicacion.Solicitudes;

public record ListadoSolicitudesResultado(IReadOnlyList<Solicitud> Items, int Total);
