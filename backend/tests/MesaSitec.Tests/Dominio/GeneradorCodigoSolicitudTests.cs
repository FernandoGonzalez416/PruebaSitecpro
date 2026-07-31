using MesaSitec.Dominio.Reglas;

namespace MesaSitec.Tests.Dominio;

public class GeneradorCodigoSolicitudTests
{
    [Fact]
    public void Generar_SinSolicitudesEnElAnio_DevuelveCorrelativoInicial()
    {
        var codigo = GeneradorCodigoSolicitud.Generar(anio: 2026, totalEnAnio: 0);

        Assert.Equal("SOL-2026-00001", codigo);
    }

    [Fact]
    public void Generar_TotalVeinticinco_DevuelveVeintiseis()
    {
        var codigo = GeneradorCodigoSolicitud.Generar(anio: 2026, totalEnAnio: 25);

        Assert.Equal("SOL-2026-00026", codigo);
    }

    [Fact]
    public void Generar_TotalOcho_DevuelveNueveConPadding()
    {
        var codigo = GeneradorCodigoSolicitud.Generar(anio: 2026, totalEnAnio: 8);

        Assert.Equal("SOL-2026-00009", codigo);
    }

    [Fact]
    public void Generar_CambioDeAnio_ReiniciaCorrelativo()
    {
        var codigo = GeneradorCodigoSolicitud.Generar(anio: 2027, totalEnAnio: 0);

        Assert.Equal("SOL-2027-00001", codigo);
    }

    [Fact]
    public void Generar_AnioYOrganizacionIndependientes_NoInfluencianTotal()
    {
        var norte = GeneradorCodigoSolicitud.Generar(anio: 2026, totalEnAnio: 25);
        var sur = GeneradorCodigoSolicitud.Generar(anio: 2026, totalEnAnio: 8);

        Assert.Equal("SOL-2026-00026", norte);
        Assert.Equal("SOL-2026-00009", sur);
    }
}
