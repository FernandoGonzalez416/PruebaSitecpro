using HelpDesk.Aplicacion.Solicitudes;
using HelpDesk.Dominio.Excepciones;
using HelpDesk.Dominio.Entidades;
using HelpDesk.Infraestructura.Data;
using HelpDesk.Infraestructura.Solicitudes;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace HelpDesk.Tests.Aplicacion;

public class SolicitudServicioErroresTests : IDisposable
{
    private readonly SqliteConnection _conexion;
    private readonly DbContextOptions<HelpDeskDbContext> _opciones;

    public SolicitudServicioErroresTests()
    {
        _conexion = new SqliteConnection("DataSource=:memory:");
        _conexion.Open();
        _opciones = new DbContextOptionsBuilder<HelpDeskDbContext>()
            .UseSqlite(_conexion)
            .Options;

        using (var db = new HelpDeskDbContext(_opciones))
        {
            db.Database.EnsureCreated();
        }
    }

    public void Dispose() => _conexion.Dispose();

    [Fact]
    public async Task ListarAsync_PageFueraDeRango_LanzaParametroInvalidoConErrores()
    {
        var tenantId = Guid.NewGuid();

        using (var db = new HelpDeskDbContext(_opciones))
        {
            var servicio = new SolicitudServicio(new SolicitudDatos(db));

            var excepcion = await Assert.ThrowsAsync<ExcepcionNegocio>(() =>
                servicio.ListarAsync(
                    new ConsultaListadoSolicitudes(
                        Estado: null,
                        Prioridad: null,
                        CategoriaId: null,
                        AgenteId: null,
                        Q: null,
                        Vencidas: false,
                        Page: 0,
                        PageSize: 20,
                        Sort: "fechaCreacion"),
                    tenantId,
                    RolUsuario.Admin,
                    Guid.NewGuid()));

            Assert.Equal("PARAMETRO_INVALIDO", excepcion.Codigo);
            Assert.NotNull(excepcion.Errores);
            Assert.Contains("page", excepcion.Errores!.Keys);
            Assert.Contains("page debe ser mayor o igual a 1.", excepcion.Errores["page"]);
        }
    }

    [Fact]
    public async Task ListarAsync_SortInvalido_LanzaParametroInvalidoConErrores()
    {
        var tenantId = Guid.NewGuid();

        using (var db = new HelpDeskDbContext(_opciones))
        {
            var servicio = new SolicitudServicio(new SolicitudDatos(db));

            var excepcion = await Assert.ThrowsAsync<ExcepcionNegocio>(() =>
                servicio.ListarAsync(
                    new ConsultaListadoSolicitudes(
                        Estado: null,
                        Prioridad: null,
                        CategoriaId: null,
                        AgenteId: null,
                        Q: null,
                        Vencidas: false,
                        Page: 1,
                        PageSize: 20,
                        Sort: "xyz"),
                    tenantId,
                    RolUsuario.Admin,
                    Guid.NewGuid()));

            Assert.Equal("PARAMETRO_INVALIDO", excepcion.Codigo);
            Assert.NotNull(excepcion.Errores);
            Assert.Contains("sort", excepcion.Errores!.Keys);
            Assert.Contains("El parámetro sort no es válido.", excepcion.Errores["sort"]);
        }
    }
}
