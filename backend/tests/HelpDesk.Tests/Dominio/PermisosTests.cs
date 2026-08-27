using HelpDesk.Dominio.Entidades;
using HelpDesk.Dominio.Excepciones;
using HelpDesk.Dominio.Reglas;

namespace HelpDesk.Tests.Dominio;

public class PermisosTests
{
    [Fact]
    public void Verificar_SolicitanteIntentaVerSolicitudAjena_LanzaExcepcionConCodigoDelContrato()
    {
        var excepcion = Assert.Throws<ExcepcionOperacionNoPermitida>(
            () => PermisosSolicitud.Verificar(
                RolUsuario.Solicitante, AccionesSolicitud.Ver, esPropia: false, EstadoSolicitud.Nueva));

        Assert.Equal("OPERACION_NO_PERMITIDA", excepcion.Codigo);
        Assert.Equal(403, excepcion.Status);
    }

    [Fact]
    public void Verificar_AgenteIntentaCancelar_LanzaExcepcion()
    {
        Assert.Throws<ExcepcionOperacionNoPermitida>(
            () => PermisosSolicitud.Verificar(
                RolUsuario.Agente, AccionesSolicitud.Cancelar, esPropia: true, EstadoSolicitud.EnProceso));
    }

    [Fact]
    public void Verificar_AdminIntentaCancelar_NoLanzaExcepcion()
    {
        PermisosSolicitud.Verificar(
            RolUsuario.Admin, AccionesSolicitud.Cancelar, esPropia: true, EstadoSolicitud.EnProceso);
    }

    [Fact]
    public void Verificar_SolicitanteVerPropia_NoLanzaExcepcion()
    {
        PermisosSolicitud.Verificar(
            RolUsuario.Solicitante, AccionesSolicitud.Ver, esPropia: true, EstadoSolicitud.Nueva);
    }

    [Fact]
    public void Verificar_SolicitanteEditarPropiaEnNueva_NoLanzaExcepcion()
    {
        PermisosSolicitud.Verificar(
            RolUsuario.Solicitante, AccionesSolicitud.Editar, esPropia: true, EstadoSolicitud.Nueva);
    }

    [Fact]
    public void Verificar_SolicitanteEditarPropiaFueraDeNueva_LanzaExcepcion()
    {
        Assert.Throws<ExcepcionOperacionNoPermitida>(
            () => PermisosSolicitud.Verificar(
                RolUsuario.Solicitante, AccionesSolicitud.Editar, esPropia: true, EstadoSolicitud.Asignada));
    }

    [Fact]
    public void Verificar_SolicitanteEditarAjenaEnNueva_LanzaExcepcion()
    {
        Assert.Throws<ExcepcionOperacionNoPermitida>(
            () => PermisosSolicitud.Verificar(
                RolUsuario.Solicitante, AccionesSolicitud.Editar, esPropia: false, EstadoSolicitud.Nueva));
    }

    [Fact]
    public void Verificar_SolicitanteCerrarPropia_NoLanzaExcepcion()
    {
        PermisosSolicitud.Verificar(
            RolUsuario.Solicitante, AccionesSolicitud.Cerrar, esPropia: true, EstadoSolicitud.Resuelta);
    }

    [Fact]
    public void Verificar_AgenteAsignar_NoLanzaExcepcion()
    {
        PermisosSolicitud.Verificar(
            RolUsuario.Agente, AccionesSolicitud.Asignar, esPropia: false, EstadoSolicitud.Nueva);
    }

    [Fact]
    public void Verificar_SolicitanteIntentaAsignar_LanzaExcepcion()
    {
        Assert.Throws<ExcepcionOperacionNoPermitida>(
            () => PermisosSolicitud.Verificar(
                RolUsuario.Solicitante, AccionesSolicitud.Asignar, esPropia: false, EstadoSolicitud.Nueva));
    }
}
