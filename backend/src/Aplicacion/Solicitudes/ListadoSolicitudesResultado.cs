using MesaSitec.Dominio.Entidades;

namespace MesaSitec.Aplicacion.Solicitudes;

public record ListadoSolicitudesResultado(IReadOnlyList<Solicitud> Items, int Total);
