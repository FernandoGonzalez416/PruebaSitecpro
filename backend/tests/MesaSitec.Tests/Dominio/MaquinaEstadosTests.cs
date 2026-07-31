using MesaSitec.Dominio.Entidades;
using MesaSitec.Dominio.Excepciones;
using MesaSitec.Dominio.Reglas;

namespace MesaSitec.Tests.Dominio;

public class MaquinaEstadosTests
{
    [Theory]
    [InlineData(EstadoSolicitud.Nueva, AccionesSolicitud.Asignar, EstadoSolicitud.Asignada)]
    [InlineData(EstadoSolicitud.Nueva, AccionesSolicitud.Cancelar, EstadoSolicitud.Cancelada)]
    [InlineData(EstadoSolicitud.Asignada, AccionesSolicitud.Iniciar, EstadoSolicitud.EnProceso)]
    [InlineData(EstadoSolicitud.Asignada, AccionesSolicitud.Asignar, EstadoSolicitud.Asignada)]
    [InlineData(EstadoSolicitud.Asignada, AccionesSolicitud.Cancelar, EstadoSolicitud.Cancelada)]
    [InlineData(EstadoSolicitud.EnProceso, AccionesSolicitud.Resolver, EstadoSolicitud.Resuelta)]
    [InlineData(EstadoSolicitud.EnProceso, AccionesSolicitud.Asignar, EstadoSolicitud.Asignada)]
    [InlineData(EstadoSolicitud.EnProceso, AccionesSolicitud.Cancelar, EstadoSolicitud.Cancelada)]
    [InlineData(EstadoSolicitud.Resuelta, AccionesSolicitud.Cerrar, EstadoSolicitud.Cerrada)]
    [InlineData(EstadoSolicitud.Resuelta, AccionesSolicitud.Reabrir, EstadoSolicitud.EnProceso)]
    public void AplicarAccion_TransicionValida_DevuelveEstadoDestino(
        EstadoSolicitud estadoActual, string accion, EstadoSolicitud estadoEsperado)
    {
        var resultado = MaquinaEstadosSolicitud.AplicarAccion(estadoActual, accion);

        Assert.Equal(estadoEsperado, resultado);
    }

    [Theory]
    [InlineData(EstadoSolicitud.Nueva, AccionesSolicitud.Iniciar)]
    [InlineData(EstadoSolicitud.Nueva, AccionesSolicitud.Resolver)]
    [InlineData(EstadoSolicitud.Nueva, AccionesSolicitud.Cerrar)]
    [InlineData(EstadoSolicitud.Asignada, AccionesSolicitud.Resolver)]
    [InlineData(EstadoSolicitud.Asignada, AccionesSolicitud.Cerrar)]
    [InlineData(EstadoSolicitud.EnProceso, AccionesSolicitud.Iniciar)]
    [InlineData(EstadoSolicitud.EnProceso, AccionesSolicitud.Cerrar)]
    [InlineData(EstadoSolicitud.Resuelta, AccionesSolicitud.Asignar)]
    [InlineData(EstadoSolicitud.Resuelta, AccionesSolicitud.Resolver)]
    [InlineData(EstadoSolicitud.Cerrada, AccionesSolicitud.Asignar)]
    [InlineData(EstadoSolicitud.Cerrada, AccionesSolicitud.Reabrir)]
    [InlineData(EstadoSolicitud.Cancelada, AccionesSolicitud.Cerrar)]
    [InlineData(EstadoSolicitud.Cancelada, AccionesSolicitud.Reabrir)]
    public void AplicarAccion_TransicionInvalida_LanzaExcepcionConCodigoDelContrato(
        EstadoSolicitud estadoActual, string accion)
    {
        var excepcion = Assert.Throws<ExcepcionTransicionInvalida>(
            () => MaquinaEstadosSolicitud.AplicarAccion(estadoActual, accion));

        Assert.Equal("TRANSICION_INVALIDA", excepcion.Codigo);
        Assert.Equal(409, excepcion.Status);
    }
}
