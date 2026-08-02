using MesaSitec.Aplicacion.Solicitudes;
using MesaSitec.Aplicacion.Solicitudes.DTOs;
using MesaSitec.Dominio.Entidades;
using MesaSitec.Dominio.Reglas;
using MesaSitec.Infraestructura.Data;
using MesaSitec.Infraestructura.Solicitudes;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace MesaSitec.Tests.Aplicacion;

public class SolicitudServicioTests : IDisposable
{
    private readonly SqliteConnection _conexion;
    private readonly DbContextOptions<MesaSitecDbContext> _opciones;

    public SolicitudServicioTests()
    {
        _conexion = new SqliteConnection("DataSource=:memory:");
        _conexion.Open();
        _opciones = new DbContextOptionsBuilder<MesaSitecDbContext>()
            .UseSqlite(_conexion)
            .Options;

        using (var db = new MesaSitecDbContext(_opciones))
        {
            db.Database.EnsureCreated();
        }
    }

    public void Dispose() => _conexion.Dispose();

    [Fact]
    public async Task ActualizarAsync_CambiarCategoria_RecalculaSlaYDevuelveDetalleCompleto()
    {
        var tenantId = Guid.NewGuid();
        var solicitanteId = Guid.NewGuid();
        var incidente = new Categoria { Id = Guid.NewGuid(), TenantId = tenantId, Nombre = "Incidente", SlaHoras = 8, Activo = true };
        var consulta = new Categoria { Id = Guid.NewGuid(), TenantId = tenantId, Nombre = "Consulta", SlaHoras = 24, Activo = true };
        var solicitudId = Guid.NewGuid();
        var fechaCreacion = new DateTime(2026, 1, 15, 8, 0, 0, DateTimeKind.Utc);

        using (var db = new MesaSitecDbContext(_opciones))
        {
            db.Tenants.Add(new Tenant { Id = tenantId, Nombre = "Cooperativa Norte", Activo = true });
            db.Usuarios.Add(new Usuario
            {
                Id = solicitanteId,
                TenantId = tenantId,
                Email = "user1@norte.test",
                PasswordHash = "x",
                Nombre = "Usuario Uno Norte",
                Rol = RolUsuario.Solicitante,
                Activo = true
            });
            db.Categorias.AddRange(incidente, consulta);
            db.Solicitudes.Add(new Solicitud
            {
                Id = solicitudId,
                TenantId = tenantId,
                Codigo = "SOL-2026-00001",
                Titulo = "No puedo acceder al portal",
                Descripcion = "Al ingresar mis credenciales el sistema me devuelve a la pantalla de login.",
                Estado = EstadoSolicitud.Nueva,
                Prioridad = Prioridad.Media,
                CategoriaId = incidente.Id,
                SolicitanteId = solicitanteId,
                FechaCreacion = fechaCreacion,
                FechaLimiteSla = CalculadorSla.Calcular(fechaCreacion, incidente.SlaHoras, Prioridad.Media)
            });
            await db.SaveChangesAsync();
        }

        using (var db = new MesaSitecDbContext(_opciones))
        {
            var servicio = new SolicitudServicio(new SolicitudDatos(db));

            var dto = await servicio.ActualizarAsync(
                solicitudId,
                new SolicitudRequest
                {
                    Titulo = "No puedo acceder al portal",
                    Descripcion = "Al ingresar mis credenciales el sistema me devuelve a la pantalla de login.",
                    CategoriaId = consulta.Id,
                    Prioridad = Prioridad.Critica
                },
                tenantId,
                RolUsuario.Admin,
                solicitanteId);

            Assert.Equal("Consulta", dto.Categoria.Nombre);
            Assert.Equal(Prioridad.Critica, dto.Prioridad);
            Assert.Equal("Usuario Uno Norte", dto.Solicitante.Nombre);
            Assert.Equal(fechaCreacion, dto.FechaCreacion);
            Assert.Equal(fechaCreacion.AddHours(12), dto.FechaLimiteSla);
        }
    }

    [Fact]
    public async Task EjecutarTransicionAsync_Reasignar_DevuelveDetalleConNuevoAgente()
    {
        var tenantId = Guid.NewGuid();
        var solicitanteId = Guid.NewGuid();
        var agente1 = new Usuario { Id = Guid.NewGuid(), TenantId = tenantId, Email = "agente1@norte.test", PasswordHash = "x", Nombre = "Agente Uno Norte", Rol = RolUsuario.Agente, Activo = true };
        var agente2 = new Usuario { Id = Guid.NewGuid(), TenantId = tenantId, Email = "agente2@norte.test", PasswordHash = "x", Nombre = "Agente Dos Norte", Rol = RolUsuario.Agente, Activo = true };
        var categoria = new Categoria { Id = Guid.NewGuid(), TenantId = tenantId, Nombre = "Incidente", SlaHoras = 8, Activo = true };
        var solicitudId = Guid.NewGuid();
        var fechaCreacion = new DateTime(2026, 1, 15, 8, 0, 0, DateTimeKind.Utc);

        using (var db = new MesaSitecDbContext(_opciones))
        {
            db.Tenants.Add(new Tenant { Id = tenantId, Nombre = "Cooperativa Norte", Activo = true });
            db.Usuarios.AddRange(agente1, agente2, new Usuario
            {
                Id = solicitanteId,
                TenantId = tenantId,
                Email = "user1@norte.test",
                PasswordHash = "x",
                Nombre = "Usuario Uno Norte",
                Rol = RolUsuario.Solicitante,
                Activo = true
            });
            db.Categorias.Add(categoria);
            db.Solicitudes.Add(new Solicitud
            {
                Id = solicitudId,
                TenantId = tenantId,
                Codigo = "SOL-2026-00001",
                Titulo = "No puedo acceder al portal",
                Descripcion = "Al ingresar mis credenciales el sistema me devuelve a la pantalla de login.",
                Estado = EstadoSolicitud.Asignada,
                Prioridad = Prioridad.Alta,
                CategoriaId = categoria.Id,
                SolicitanteId = solicitanteId,
                AgenteId = agente1.Id,
                FechaCreacion = fechaCreacion,
                FechaLimiteSla = CalculadorSla.Calcular(fechaCreacion, categoria.SlaHoras, Prioridad.Alta)
            });
            await db.SaveChangesAsync();
        }

        using (var db = new MesaSitecDbContext(_opciones))
        {
            var servicio = new SolicitudServicio(new SolicitudDatos(db));

            var dto = await servicio.EjecutarTransicionAsync(
                solicitudId,
                new TransicionRequest { Accion = AccionesSolicitud.Asignar, AgenteId = agente2.Id },
                tenantId,
                RolUsuario.Admin,
                solicitanteId);

            Assert.Equal(EstadoSolicitud.Asignada, dto.Estado);
            Assert.Equal("Agente Dos Norte", dto.Agente!.Nombre);
        }
    }

    [Fact]
    public async Task EjecutarTransicionAsync_ReasignarMismoAgente_NoLanzaYConservaAgente()
    {
        var tenantId = Guid.NewGuid();
        var solicitanteId = Guid.NewGuid();
        var agente = new Usuario { Id = Guid.NewGuid(), TenantId = tenantId, Email = "agente1@norte.test", PasswordHash = "x", Nombre = "Agente Uno Norte", Rol = RolUsuario.Agente, Activo = true };
        var categoria = new Categoria { Id = Guid.NewGuid(), TenantId = tenantId, Nombre = "Incidente", SlaHoras = 8, Activo = true };
        var solicitudId = Guid.NewGuid();
        var fechaCreacion = new DateTime(2026, 1, 15, 8, 0, 0, DateTimeKind.Utc);

        using (var db = new MesaSitecDbContext(_opciones))
        {
            db.Tenants.Add(new Tenant { Id = tenantId, Nombre = "Cooperativa Norte", Activo = true });
            db.Usuarios.AddRange(agente, new Usuario
            {
                Id = solicitanteId,
                TenantId = tenantId,
                Email = "user1@norte.test",
                PasswordHash = "x",
                Nombre = "Usuario Uno Norte",
                Rol = RolUsuario.Solicitante,
                Activo = true
            });
            db.Categorias.Add(categoria);
            db.Solicitudes.Add(new Solicitud
            {
                Id = solicitudId,
                TenantId = tenantId,
                Codigo = "SOL-2026-00001",
                Titulo = "No puedo acceder al portal",
                Descripcion = "Al ingresar mis credenciales el sistema me devuelve a la pantalla de login.",
                Estado = EstadoSolicitud.Asignada,
                Prioridad = Prioridad.Alta,
                CategoriaId = categoria.Id,
                SolicitanteId = solicitanteId,
                AgenteId = agente.Id,
                FechaCreacion = fechaCreacion,
                FechaLimiteSla = CalculadorSla.Calcular(fechaCreacion, categoria.SlaHoras, Prioridad.Alta)
            });
            await db.SaveChangesAsync();
        }

        using (var db = new MesaSitecDbContext(_opciones))
        {
            var servicio = new SolicitudServicio(new SolicitudDatos(db));

            var dto = await servicio.EjecutarTransicionAsync(
                solicitudId,
                new TransicionRequest { Accion = AccionesSolicitud.Asignar, AgenteId = agente.Id },
                tenantId,
                RolUsuario.Admin,
                solicitanteId);

            Assert.Equal(EstadoSolicitud.Asignada, dto.Estado);
            Assert.Equal("Agente Uno Norte", dto.Agente!.Nombre);
        }
    }
}
