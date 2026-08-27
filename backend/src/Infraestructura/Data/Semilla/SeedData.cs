using System.Globalization;
using HelpDesk.Dominio.Entidades;
using HelpDesk.Infraestructura.Data;
using Microsoft.EntityFrameworkCore;

namespace HelpDesk.Infraestructura.Data.Semilla;

public static class SeedData
{
    private const string FechaBaseDefault = "2026-01-15T08:00:00Z";

    private const string PasswordSemilla = "Sitec.2026";
    private const string SaltSemilla = "$2a$11$Sitec2026SeedSaltXXX00";

    public static async Task SembrarAsync(HelpDeskDbContext db)
    {
        if (await db.Tenants.AnyAsync())
        {
            return;
        }

        var fechaBase = ObtenerFechaBase();
        var passwordHash = BCrypt.Net.BCrypt.HashPassword(PasswordSemilla, SaltSemilla);

        db.Tenants.AddRange(CrearTenants());
        db.Usuarios.AddRange(CrearUsuarios(passwordHash));
        db.Categorias.AddRange(CrearCategorias());
        db.Solicitudes.AddRange(CrearSolicitudes(fechaBase));

        await db.SaveChangesAsync();
    }

    private static DateTime ObtenerFechaBase()
    {
        var valor = Environment.GetEnvironmentVariable("SEED_FECHA_BASE") ?? FechaBaseDefault;
        return DateTime.Parse(valor, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind);
    }

    private static List<Tenant> CrearTenants() => new()
    {
        new Tenant { Id = SeedIds.TenantNorte, Nombre = "Cooperativa Norte", Activo = true },
        new Tenant { Id = SeedIds.TenantSur, Nombre = "Bufete Sur", Activo = true }
    };

    private static List<Usuario> CrearUsuarios(string passwordHash) => new()
    {
        new Usuario { Id = SeedIds.UsuarioAdminNorte, TenantId = SeedIds.TenantNorte, Email = "admin@norte.test", PasswordHash = passwordHash, Nombre = "Administrador Norte", Rol = RolUsuario.Admin, Activo = true },
        new Usuario { Id = SeedIds.UsuarioAgente1Norte, TenantId = SeedIds.TenantNorte, Email = "agente1@norte.test", PasswordHash = passwordHash, Nombre = "Agente Uno Norte", Rol = RolUsuario.Agente, Activo = true },
        new Usuario { Id = SeedIds.UsuarioAgente2Norte, TenantId = SeedIds.TenantNorte, Email = "agente2@norte.test", PasswordHash = passwordHash, Nombre = "Agente Dos Norte", Rol = RolUsuario.Agente, Activo = true },
        new Usuario { Id = SeedIds.UsuarioUser1Norte, TenantId = SeedIds.TenantNorte, Email = "user1@norte.test", PasswordHash = passwordHash, Nombre = "Usuario Uno Norte", Rol = RolUsuario.Solicitante, Activo = true },
        new Usuario { Id = SeedIds.UsuarioUser2Norte, TenantId = SeedIds.TenantNorte, Email = "user2@norte.test", PasswordHash = passwordHash, Nombre = "Usuario Dos Norte", Rol = RolUsuario.Solicitante, Activo = true },
        new Usuario { Id = SeedIds.UsuarioAdminSur, TenantId = SeedIds.TenantSur, Email = "admin@sur.test", PasswordHash = passwordHash, Nombre = "Administrador Sur", Rol = RolUsuario.Admin, Activo = true },
        new Usuario { Id = SeedIds.UsuarioUser1Sur, TenantId = SeedIds.TenantSur, Email = "user1@sur.test", PasswordHash = passwordHash, Nombre = "Usuario Uno Sur", Rol = RolUsuario.Solicitante, Activo = true }
    };

    private static List<Categoria> CrearCategorias() => new()
    {
        new Categoria { Id = SeedIds.CategoriaIncidenteNorte, TenantId = SeedIds.TenantNorte, Nombre = "Incidente", SlaHoras = 8, Activo = true },
        new Categoria { Id = SeedIds.CategoriaRequerimientoNorte, TenantId = SeedIds.TenantNorte, Nombre = "Requerimiento", SlaHoras = 40, Activo = true },
        new Categoria { Id = SeedIds.CategoriaConsultaNorte, TenantId = SeedIds.TenantNorte, Nombre = "Consulta", SlaHoras = 24, Activo = true },
        new Categoria { Id = SeedIds.CategoriaFallaCriticaNorte, TenantId = SeedIds.TenantNorte, Nombre = "Falla crítica", SlaHoras = 4, Activo = true },
        new Categoria { Id = SeedIds.CategoriaIncidenteSur, TenantId = SeedIds.TenantSur, Nombre = "Incidente", SlaHoras = 8, Activo = true },
        new Categoria { Id = SeedIds.CategoriaRequerimientoSur, TenantId = SeedIds.TenantSur, Nombre = "Requerimiento", SlaHoras = 40, Activo = true },
        new Categoria { Id = SeedIds.CategoriaConsultaSur, TenantId = SeedIds.TenantSur, Nombre = "Consulta", SlaHoras = 24, Activo = true },
        new Categoria { Id = SeedIds.CategoriaFallaCriticaSur, TenantId = SeedIds.TenantSur, Nombre = "Falla crítica", SlaHoras = 4, Activo = true }
    };

    private static List<Solicitud> CrearSolicitudes(DateTime fechaBase)
    {
        var solicitudes = new List<Solicitud>(SolicitudesNorte.Length + SolicitudesSur.Length);
        solicitudes.AddRange(SolicitudesNorte.Select((s, i) => ConstruirSolicitud(s, fechaBase, SeedIds.TenantNorte, IdSolicitud(i + 1, esNorte: true))));
        solicitudes.AddRange(SolicitudesSur.Select((s, i) => ConstruirSolicitud(s, fechaBase, SeedIds.TenantSur, IdSolicitud(i + 1, esNorte: false))));
        return solicitudes;
    }

    private static Solicitud ConstruirSolicitud(SolicitudSemilla s, DateTime fechaBase, Guid tenantId, Guid id)
    {
        var fechaCreacion = fechaBase.AddHours(-s.OffsetHoras);
        var fechaLimiteSla = CalcularFechaLimite(fechaCreacion, SlaHorasPorCategoria[s.CategoriaId], s.Prioridad);

        return new Solicitud
        {
            Id = id,
            TenantId = tenantId,
            Codigo = s.Codigo,
            Titulo = s.Titulo,
            Descripcion = s.Descripcion,
            Estado = s.Estado,
            Prioridad = s.Prioridad,
            CategoriaId = s.CategoriaId,
            SolicitanteId = s.SolicitanteId,
            AgenteId = s.AgenteId,
            FechaCreacion = fechaCreacion,
            FechaLimiteSla = fechaLimiteSla,
            FechaResolucion = s.OffsetHorasResolucion is int o ? fechaBase.AddHours(-o) : null,
            MotivoResolucion = s.MotivoResolucion,
            MotivoCancelacion = s.MotivoCancelacion
        };
    }

    private static Guid IdSolicitud(int correlativo, bool esNorte) =>
        Guid.Parse($"{(esNorte ? "50000000" : "60000000")}-0000-0000-0000-{correlativo:D12}");

    private static DateTime CalcularFechaLimite(DateTime fechaCreacion, int slaHoras, Prioridad prioridad)
    {
        var factor = prioridad switch
        {
            Prioridad.Critica => 0.5,
            Prioridad.Alta => 0.75,
            Prioridad.Media => 1.0,
            Prioridad.Baja => 2.0,
            _ => throw new ArgumentOutOfRangeException(nameof(prioridad), prioridad, null)
        };

        return fechaCreacion.AddHours(slaHoras * factor);
    }

    private static readonly IReadOnlyDictionary<Guid, int> SlaHorasPorCategoria = new Dictionary<Guid, int>
    {
        [SeedIds.CategoriaIncidenteNorte] = 8,
        [SeedIds.CategoriaRequerimientoNorte] = 40,
        [SeedIds.CategoriaConsultaNorte] = 24,
        [SeedIds.CategoriaFallaCriticaNorte] = 4,
        [SeedIds.CategoriaIncidenteSur] = 8,
        [SeedIds.CategoriaRequerimientoSur] = 40,
        [SeedIds.CategoriaConsultaSur] = 24,
        [SeedIds.CategoriaFallaCriticaSur] = 4
    };

    private sealed record SolicitudSemilla(
        string Codigo,
        string Titulo,
        string Descripcion,
        Guid CategoriaId,
        Prioridad Prioridad,
        EstadoSolicitud Estado,
        Guid SolicitanteId,
        Guid? AgenteId,
        int OffsetHoras,
        int? OffsetHorasResolucion,
        string? MotivoResolucion,
        string? MotivoCancelacion);

    private static readonly SolicitudSemilla[] SolicitudesNorte =
    {
        new("SOL-2026-00001", "Sistema de facturación sin servicio", "El módulo de facturación no responde y los clientes no pueden generar comprobantes desde esta mañana.", SeedIds.CategoriaFallaCriticaNorte, Prioridad.Critica, EstadoSolicitud.Nueva, SeedIds.UsuarioUser1Norte, null, 144, null, null, null),
        new("SOL-2026-00002", "Impresora de recibos en falla", "La impresora del área de caja muestra error de conexión y no imprime los recibos de pago.", SeedIds.CategoriaFallaCriticaNorte, Prioridad.Media, EstadoSolicitud.Nueva, SeedIds.UsuarioUser2Norte, null, 144, null, null, null),
        new("SOL-2026-00003", "No puedo acceder al portal", "Al ingresar mis credenciales el sistema me devuelve a la pantalla de login sin mostrar ningún mensaje.", SeedIds.CategoriaIncidenteNorte, Prioridad.Alta, EstadoSolicitud.Asignada, SeedIds.UsuarioUser1Norte, SeedIds.UsuarioAgente1Norte, 144, null, null, null),
        new("SOL-2026-00004", "Correo institucional caído", "No recibo ni envío correos desde el buzón institucional; los mensajes quedan en cola de salida.", SeedIds.CategoriaIncidenteNorte, Prioridad.Media, EstadoSolicitud.Asignada, SeedIds.UsuarioUser2Norte, SeedIds.UsuarioAgente2Norte, 144, null, null, null),
        new("SOL-2026-00005", "Duda sobre política de vacaciones", "Necesito confirmar cuántos días de vacaciones tengo disponibles según la nueva política aprobada.", SeedIds.CategoriaConsultaNorte, Prioridad.Critica, EstadoSolicitud.EnProceso, SeedIds.UsuarioUser1Norte, SeedIds.UsuarioAgente1Norte, 144, null, null, null),
        new("SOL-2026-00006", "Icono de notificaciones no actualiza", "El contador de notificaciones no refleja los cambios aunque ya leí los mensajes nuevos.", SeedIds.CategoriaIncidenteNorte, Prioridad.Baja, EstadoSolicitud.Nueva, SeedIds.UsuarioUser2Norte, null, 8, null, null, null),
        new("SOL-2026-00007", "Acceso a carpeta compartida", "Solicito permisos de lectura y escritura en la carpeta compartida del proyecto de auditoría.", SeedIds.CategoriaRequerimientoNorte, Prioridad.Media, EstadoSolicitud.Nueva, SeedIds.UsuarioUser1Norte, null, 24, null, null, null),
        new("SOL-2026-00008", "Guía para cambio de contraseña", "Quisiera conocer el procedimiento para renovar la contraseña corporativa cuando caduque.", SeedIds.CategoriaConsultaNorte, Prioridad.Baja, EstadoSolicitud.Nueva, SeedIds.UsuarioUser2Norte, null, 48, null, null, null),
        new("SOL-2026-00009", "Error al guardar documento", "Al guardar un documento en la plataforma aparece un error 500 y se pierde la información escrita.", SeedIds.CategoriaFallaCriticaNorte, Prioridad.Alta, EstadoSolicitud.Asignada, SeedIds.UsuarioUser1Norte, SeedIds.UsuarioAgente2Norte, 2, null, null, null),
        new("SOL-2026-00010", "Archivo adjunto muy lento", "Los archivos adjuntos tardan demasiado en cargarse cuando superan los cinco megabytes.", SeedIds.CategoriaIncidenteNorte, Prioridad.Media, EstadoSolicitud.Asignada, SeedIds.UsuarioUser2Norte, SeedIds.UsuarioAgente1Norte, 4, null, null, null),
        new("SOL-2026-00011", "Instalar herramienta de diseño", "Necesito que se instale el editor de diagramas en mi equipo para la documentación de procesos.", SeedIds.CategoriaRequerimientoNorte, Prioridad.Baja, EstadoSolicitud.Asignada, SeedIds.UsuarioUser1Norte, SeedIds.UsuarioAgente2Norte, 48, null, null, null),
        new("SOL-2026-00012", "Consulta sobre horarios flexibles", "Quiero saber si mi área puede acogerse a la modalidad de horario flexible este trimestre.", SeedIds.CategoriaConsultaNorte, Prioridad.Alta, EstadoSolicitud.EnProceso, SeedIds.UsuarioUser2Norte, SeedIds.UsuarioAgente1Norte, 12, null, null, null),
        new("SOL-2026-00013", "Base de datos de ventas lenta", "Las consultas del módulo de ventas tardan más de un minuto y bloquean el trabajo del equipo.", SeedIds.CategoriaIncidenteNorte, Prioridad.Critica, EstadoSolicitud.EnProceso, SeedIds.UsuarioUser1Norte, SeedIds.UsuarioAgente2Norte, 3, null, null, null),
        new("SOL-2026-00014", "Nuevo acceso a reportes", "Solicito acceso al panel de reportes gerenciales para preparar la información del consejo.", SeedIds.CategoriaRequerimientoNorte, Prioridad.Alta, EstadoSolicitud.EnProceso, SeedIds.UsuarioUser2Norte, SeedIds.UsuarioAgente1Norte, 20, null, null, null),
        new("SOL-2026-00015", "Certificado SSL vence pronto", "El certificado del portal institucional vence en pocos días y pide renovación anticipada.", SeedIds.CategoriaFallaCriticaNorte, Prioridad.Baja, EstadoSolicitud.EnProceso, SeedIds.UsuarioUser1Norte, SeedIds.UsuarioAgente1Norte, 6, null, null, null),
        new("SOL-2026-00016", "Portal no carga reportes", "El portal corporativo no muestra los reportes del mes y el equipo directivo los necesita para la revisión.", SeedIds.CategoriaIncidenteNorte, Prioridad.Alta, EstadoSolicitud.Resuelta, SeedIds.UsuarioUser1Norte, SeedIds.UsuarioAgente1Norte, 120, 96, "Se restableció el acceso al portal y se verificó que el usuario puede ingresar correctamente.", null),
        new("SOL-2026-00017", "Buzón corporativo saturado", "El buzón de correo llegó a su cuota máxima y no se reciben mensajes nuevos de los clientes.", SeedIds.CategoriaIncidenteNorte, Prioridad.Media, EstadoSolicitud.Resuelta, SeedIds.UsuarioUser2Norte, SeedIds.UsuarioAgente2Norte, 96, 72, "Se actualizó el correo corporativo y se probó el inicio de sesión con éxito desde dos equipos.", null),
        new("SOL-2026-00018", "Consulta por política de teletrabajo", "Necesito conocer los requisitos vigentes para solicitar la modalidad de teletrabajo en mi puesto.", SeedIds.CategoriaConsultaNorte, Prioridad.Alta, EstadoSolicitud.Resuelta, SeedIds.UsuarioUser1Norte, SeedIds.UsuarioAgente1Norte, 72, 48, "Se respondió la consulta con la documentación vigente del proceso y se confirmó con el solicitante.", null),
        new("SOL-2026-00019", "Migración de equipo de trabajo", "Solicito la migración de los datos y accesos del equipo anterior a la nueva estación de trabajo.", SeedIds.CategoriaRequerimientoNorte, Prioridad.Media, EstadoSolicitud.Cerrada, SeedIds.UsuarioUser2Norte, SeedIds.UsuarioAgente2Norte, 168, 120, "Se completó el requerimiento, se realizaron las pruebas de aceptación y el cliente confirmó la entrega.", null),
        new("SOL-2026-00020", "Servicio web interno inestable", "El servicio web de integración interna falla de forma intermitente y afecta a tres aplicaciones.", SeedIds.CategoriaFallaCriticaNorte, Prioridad.Critica, EstadoSolicitud.Cerrada, SeedIds.UsuarioUser1Norte, SeedIds.UsuarioAgente1Norte, 192, 168, "Se aplicó el parche correctivo, se validó el entorno de producción y se cerró el incidente sin novedad.", null),
        new("SOL-2026-00021", "Solicitud de vacaciones duplicada", "Registré la solicitud de vacaciones dos veces por error y pido dejar solo la primera.", SeedIds.CategoriaIncidenteNorte, Prioridad.Baja, EstadoSolicitud.Cancelada, SeedIds.UsuarioUser1Norte, null, 96, null, null, "Duplicada de SOL-2026-00010."),
        new("SOL-2026-00022", "Retiro de pedido de insumos", "Ya no necesito los insumos de papelería solicitados porque se resolvió con otra área.", SeedIds.CategoriaConsultaNorte, Prioridad.Media, EstadoSolicitud.Cancelada, SeedIds.UsuarioUser2Norte, null, 72, null, null, "El usuario retiró la solicitud por propia decisión."),
        new("SOL-2026-00023", "Alta de usuario externo", "Solicité un usuario externo para el consultor, pero el acceso se gestionó por otra vía.", SeedIds.CategoriaRequerimientoNorte, Prioridad.Baja, EstadoSolicitud.Cancelada, SeedIds.UsuarioUser1Norte, null, 168, null, null, "Resuelta por otro canal sin requerir intervención."),
        new("SOL-2026-00024", "Servicio de pagos fuera de línea", "El servicio de pagos en línea no procesa transacciones y los clientes reportan rechazos inmediatos.", SeedIds.CategoriaFallaCriticaNorte, Prioridad.Alta, EstadoSolicitud.Nueva, SeedIds.UsuarioUser2Norte, null, 1, null, null, null),
        new("SOL-2026-00025", "Error al imprimir reporte", "Al generar el reporte mensual la impresión sale en blanco en todas las impresoras de la oficina.", SeedIds.CategoriaIncidenteNorte, Prioridad.Media, EstadoSolicitud.Nueva, SeedIds.UsuarioUser1Norte, null, 2, null, null, null)
    };

    private static readonly SolicitudSemilla[] SolicitudesSur =
    {
        new("SOL-2026-00001", "Portal de consultas sin respuesta", "Al ingresar al portal de consultas la página queda en blanco y no se carga el menú principal.", SeedIds.CategoriaIncidenteSur, Prioridad.Alta, EstadoSolicitud.Nueva, SeedIds.UsuarioUser1Sur, null, 48, null, null, null),
        new("SOL-2026-00002", "Actualizar datos de contacto", "Necesito actualizar mi número de teléfono y dirección en el registro del sistema.", SeedIds.CategoriaIncidenteSur, Prioridad.Baja, EstadoSolicitud.Nueva, SeedIds.UsuarioUser1Sur, null, 24, null, null, null),
        new("SOL-2026-00003", "Alta de nuevo proveedor", "Solicito el registro del nuevo proveedor de servicios de limpieza en el catálogo de pagos.", SeedIds.CategoriaRequerimientoSur, Prioridad.Media, EstadoSolicitud.Nueva, SeedIds.UsuarioUser1Sur, null, 120, null, null, null),
        new("SOL-2026-00004", "Horario de soporte técnico", "Quiero confirmar el horario de atención del soporte técnico durante las próximas semanas.", SeedIds.CategoriaConsultaSur, Prioridad.Critica, EstadoSolicitud.Nueva, SeedIds.UsuarioUser1Sur, null, 72, null, null, null),
        new("SOL-2026-00005", "No se guardan los cambios", "Al editar un expediente los cambios no se guardan y se pierde todo lo modificado.", SeedIds.CategoriaFallaCriticaSur, Prioridad.Alta, EstadoSolicitud.Nueva, SeedIds.UsuarioUser1Sur, null, 12, null, null, null),
        new("SOL-2026-00006", "Dudas sobre la liquidación", "Tengo dudas sobre cómo se calcula el ítem de viáticos en la última liquidación recibida.", SeedIds.CategoriaConsultaSur, Prioridad.Baja, EstadoSolicitud.Nueva, SeedIds.UsuarioUser1Sur, null, 96, null, null, null),
        new("SOL-2026-00007", "Acceso a sistema de archivo", "Solicito acceso al sistema de archivo digital para consultar los contratos vigentes.", SeedIds.CategoriaRequerimientoSur, Prioridad.Baja, EstadoSolicitud.Nueva, SeedIds.UsuarioUser1Sur, null, 168, null, null, null),
        new("SOL-2026-00008", "Pantalla azul en estación", "La estación de trabajo se bloquea con pantalla azul al abrir el sistema contable.", SeedIds.CategoriaFallaCriticaSur, Prioridad.Media, EstadoSolicitud.Nueva, SeedIds.UsuarioUser1Sur, null, 6, null, null, null)
    };
}
