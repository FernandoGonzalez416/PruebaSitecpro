using MesaSitec.Dominio.Entidades;

namespace MesaSitec.Dominio.Excepciones;

public class ExcepcionTransicionInvalida : ExcepcionNegocio
{
    public ExcepcionTransicionInvalida(EstadoSolicitud estadoActual, string accion)
        : base(
            codigo: "TRANSICION_INVALIDA",
            message: $"No se puede aplicar '{accion}' sobre una solicitud en estado '{estadoActual}'.",
            status: 409)
    {
    }
}
