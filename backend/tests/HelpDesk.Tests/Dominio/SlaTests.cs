using HelpDesk.Dominio.Entidades;
using HelpDesk.Dominio.Reglas;

namespace HelpDesk.Tests.Dominio;

public class SlaTests
{
    private static readonly DateTime FechaCreacion = new(2026, 1, 15, 8, 0, 0, DateTimeKind.Utc);

    [Fact]
    public void Calcular_IncidenteCritica_SumaCuatroHoras()
    {
        var resultado = CalculadorSla.Calcular(FechaCreacion, slaHoras: 8, Prioridad.Critica);

        Assert.Equal(FechaCreacion.AddHours(4), resultado);
    }

    [Fact]
    public void Calcular_ConsultaBaja_SumaCuarentaYOchoHoras()
    {
        var resultado = CalculadorSla.Calcular(FechaCreacion, slaHoras: 24, Prioridad.Baja);

        Assert.Equal(FechaCreacion.AddHours(48), resultado);
    }

    [Fact]
    public void Recalcular_CambiarPrioridad_MantieneFechaCreacionYActualizaLimite()
    {
        var solicitud = new Solicitud
        {
            FechaCreacion = FechaCreacion,
            FechaLimiteSla = CalculadorSla.Calcular(FechaCreacion, slaHoras: 8, Prioridad.Baja)
        };

        solicitud.FechaLimiteSla = CalculadorSla.Recalcular(
            solicitud.FechaCreacion, slaHoras: 8, Prioridad.Critica);

        Assert.Equal(FechaCreacion, solicitud.FechaCreacion);
        Assert.Equal(FechaCreacion.AddHours(4), solicitud.FechaLimiteSla);
    }

    [Fact]
    public void EstaVencida_LimitePasadoYEstadoNoFinal_DevuelveTrue()
    {
        var limitePasado = FechaCreacion.AddHours(4);
        var ahora = limitePasado.AddMinutes(1);

        Assert.True(CalculadorSla.EstaVencida(limitePasado, EstadoSolicitud.Asignada, ahora));
    }

    [Theory]
    [InlineData(EstadoSolicitud.Resuelta)]
    [InlineData(EstadoSolicitud.Cerrada)]
    [InlineData(EstadoSolicitud.Cancelada)]
    public void EstaVencida_LimitePasadoYEstadoFinal_DevuelveFalse(EstadoSolicitud estadoFinal)
    {
        var limitePasado = FechaCreacion.AddHours(4);
        var ahora = limitePasado.AddMinutes(1);

        Assert.False(CalculadorSla.EstaVencida(limitePasado, estadoFinal, ahora));
    }

    [Fact]
    public void EstaVencida_LimiteFuturo_DevuelveFalse()
    {
        var limiteFuturo = FechaCreacion.AddHours(4);
        var ahora = limiteFuturo.AddHours(-1);

        Assert.False(CalculadorSla.EstaVencida(limiteFuturo, EstadoSolicitud.Asignada, ahora));
    }

    [Theory]
    [InlineData(Prioridad.Critica, 0.5)]
    [InlineData(Prioridad.Alta, 0.75)]
    [InlineData(Prioridad.Media, 1.0)]
    [InlineData(Prioridad.Baja, 2.0)]
    public void Calcular_IncidenteConCadaPrioridad_AplicaFactor(Prioridad prioridad, double factor)
    {
        var esperado = FechaCreacion.AddHours(8 * factor);

        var resultado = CalculadorSla.Calcular(FechaCreacion, slaHoras: 8, prioridad);

        Assert.Equal(esperado, resultado);
    }
}
