using MesaSitec.Dominio.Entidades;

namespace MesaSitec.Dominio.Excepciones;

public class ExcepcionOperacionNoPermitida : ExcepcionNegocio
{
    public ExcepcionOperacionNoPermitida(RolUsuario rol, string accion)
        : base(
            codigo: "OPERACION_NO_PERMITIDA",
            message: $"El rol '{rol}' no tiene permiso para ejecutar '{accion}'.",
            status: 403)
    {
    }
}
