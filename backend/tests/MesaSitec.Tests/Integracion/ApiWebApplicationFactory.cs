using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;

namespace MesaSitec.Tests.Integracion;

public class ApiWebApplicationFactory : WebApplicationFactory<Program>
{
    private readonly string _rutaBaseDatos =
        Path.Combine(Path.GetTempPath(), $"mesasitec-integracion-{Guid.NewGuid():N}.db");

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Development");
        builder.ConfigureAppConfiguration((_, config) =>
        {
            config.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:Default"] = $"Data Source={_rutaBaseDatos}",
                ["JWT_SECRET"] = "clave-secreta-de-pruebas-mesasitec-que-supera-32-caracteres",
                ["JWT_ISSUER"] = "mesasitec",
                ["JWT_AUDIENCE"] = "mesasitec-client"
            });
        });
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);

        if (File.Exists(_rutaBaseDatos))
        {
            File.Delete(_rutaBaseDatos);
        }

        var wal = _rutaBaseDatos + "-wal";
        var shm = _rutaBaseDatos + "-shm";
        if (File.Exists(wal))
        {
            File.Delete(wal);
        }

        if (File.Exists(shm))
        {
            File.Delete(shm);
        }
    }
}
