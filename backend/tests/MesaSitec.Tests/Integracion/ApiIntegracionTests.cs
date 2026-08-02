using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using MesaSitec.Infraestructura.Data.Semilla;

namespace MesaSitec.Tests.Integracion;

[CollectionDefinition(ColeccionApiIntegracion.Name)]
public class ColeccionApiIntegracion : ICollectionFixture<ApiWebApplicationFactory>
{
    public const string Name = "ApiIntegracion";
}

[Collection(ColeccionApiIntegracion.Name)]
public class ApiIntegracionTests
{
    private const string PasswordSemilla = "Sitec.2026";

    private static readonly JsonSerializerOptions JsonOpciones = new()
    {
        PropertyNameCaseInsensitive = true
    };

    private static readonly Guid IdSolicitudNorte1 = Guid.Parse("50000000-0000-0000-0000-000000000001");

    private readonly ApiWebApplicationFactory _factory;
    private readonly HttpClient _cliente;

    public ApiIntegracionTests(ApiWebApplicationFactory factory)
    {
        _factory = factory;
        _cliente = factory.CreateClient();
    }

    private async Task<LoginResponseApi> LoginAsync(string email, string password = PasswordSemilla)
    {
        var respuesta = await _cliente.PostAsJsonAsync("/api/v1/auth/login", new { email, password });
        Assert.Equal(HttpStatusCode.OK, respuesta.StatusCode);
        return (await respuesta.Content.ReadFromJsonAsync<LoginResponseApi>(JsonOpciones))!;
    }

    private async Task<HttpClient> ClienteComoAsync(string email)
    {
        var login = await LoginAsync(email);
        var cliente = _factory.CreateClient();
        cliente.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", login.AccessToken);
        return cliente;
    }

    private async Task<SolicitudDetalleApi> CrearSolicitudAsync(HttpClient cliente, string titulo, string prioridad = "Baja")
    {
        var respuesta = await cliente.PostAsJsonAsync("/api/v1/solicitudes", new
        {
            titulo,
            descripcion = "Descripcion de la solicitud creada en los tests de integracion de la API.",
            categoriaId = SeedIds.CategoriaIncidenteNorte,
            prioridad
        });
        Assert.Equal(HttpStatusCode.Created, respuesta.StatusCode);
        return (await respuesta.Content.ReadFromJsonAsync<SolicitudDetalleApi>(JsonOpciones))!;
    }

    private static async Task<Problema> LeerProblemaAsync(HttpResponseMessage respuesta) =>
        (await respuesta.Content.ReadFromJsonAsync<Problema>(JsonOpciones))!;

    [Fact]
    public async Task Health_SinAutenticacion_DevuelveOk()
    {
        var respuesta = await _cliente.GetAsync("/api/v1/health");

        Assert.Equal(HttpStatusCode.OK, respuesta.StatusCode);
        var cuerpo = await respuesta.Content.ReadFromJsonAsync<HealthApi>(JsonOpciones);
        Assert.Equal("ok", cuerpo!.Estado);
    }

    [Fact]
    public async Task Login_ConCredencialesValidas_DevuelveTokenYUsuario()
    {
        var login = await LoginAsync("admin@norte.test");

        Assert.False(string.IsNullOrWhiteSpace(login.AccessToken));
        Assert.Equal(28800, login.ExpiraEn);
        Assert.Equal("admin@norte.test", login.Usuario.Email);
        Assert.Equal("Admin", login.Usuario.Rol);
        Assert.Equal(SeedIds.TenantNorte, login.Usuario.TenantId);
        Assert.Equal("Cooperativa Norte", login.Usuario.TenantNombre);
    }

    [Fact]
    public async Task Login_ConPasswordIncorrecta_Devuelve401NoAutenticado()
    {
        var respuesta = await _cliente.PostAsJsonAsync(
            "/api/v1/auth/login", new { email = "admin@norte.test", password = "incorrecta" });

        Assert.Equal(HttpStatusCode.Unauthorized, respuesta.StatusCode);
        Assert.Equal("application/problem+json", respuesta.Content.Headers.ContentType?.MediaType);
        var problema = await LeerProblemaAsync(respuesta);
        Assert.Equal("NO_AUTENTICADO", problema.Codigo);
    }

    [Fact]
    public async Task Me_ConTokenValido_DevuelveUsuarioDelToken()
    {
        var cliente = await ClienteComoAsync("user1@norte.test");
        var respuesta = await cliente.GetAsync("/api/v1/me");

        Assert.Equal(HttpStatusCode.OK, respuesta.StatusCode);
        var usuario = await respuesta.Content.ReadFromJsonAsync<UsuarioApi>(JsonOpciones);
        Assert.Equal("user1@norte.test", usuario!.Email);
        Assert.Equal("Solicitante", usuario.Rol);
    }

    [Fact]
    public async Task Me_SinToken_Devuelve401NoAutenticado()
    {
        var respuesta = await _cliente.GetAsync("/api/v1/me");

        Assert.Equal(HttpStatusCode.Unauthorized, respuesta.StatusCode);
        Assert.Equal("application/problem+json", respuesta.Content.Headers.ContentType?.MediaType);
        var problema = await LeerProblemaAsync(respuesta);
        Assert.Equal("NO_AUTENTICADO", problema.Codigo);
    }

    [Fact]
    public async Task Categorias_DevuelvenSoloLasDeLaOrganizacion()
    {
        var cliente = await ClienteComoAsync("agente1@norte.test");
        var respuesta = await cliente.GetAsync("/api/v1/categorias");

        Assert.Equal(HttpStatusCode.OK, respuesta.StatusCode);
        var categorias = await respuesta.Content.ReadFromJsonAsync<List<CategoriaApi>>(JsonOpciones);
        Assert.Equal(4, categorias!.Count);
        Assert.All(categorias, c => Assert.NotEqual(SeedIds.CategoriaIncidenteSur, c.Id));
        var incidente = categorias.Single(c => c.Nombre == "Incidente");
        Assert.Equal(8, incidente.SlaHoras);
    }

    [Fact]
    public async Task Solicitudes_ListadoPaginado_DevuelveSoloLasDeLaOrganizacion()
    {
        var cliente = await ClienteComoAsync("admin@norte.test");
        var respuesta = await cliente.GetAsync("/api/v1/solicitudes?pageSize=100");

        Assert.Equal(HttpStatusCode.OK, respuesta.StatusCode);
        var listado = await respuesta.Content.ReadFromJsonAsync<ListadoApi>(JsonOpciones);
        Assert.True(listado!.Total >= 25);
        Assert.Equal(100, listado.PageSize);
        Assert.Equal(1, listado.Page);
        Assert.All(listado.Items, i => Assert.Matches("^SOL-2026-\\d{5}$", i.Codigo));
        Assert.All(listado.Items, i => Assert.NotEqual(SeedIds.CategoriaIncidenteSur, i.Categoria.Id));
    }

    [Fact]
    public async Task Solicitudes_ListadoDeSur_SoloIncluyeLasDeSur()
    {
        var cliente = await ClienteComoAsync("admin@sur.test");
        var respuesta = await cliente.GetAsync("/api/v1/solicitudes?pageSize=100");

        Assert.Equal(HttpStatusCode.OK, respuesta.StatusCode);
        var listado = await respuesta.Content.ReadFromJsonAsync<ListadoApi>(JsonOpciones);
        Assert.Equal(8, listado!.Total);
    }

    [Fact]
    public async Task Solicitudes_FiltroPorEstado_DevuelveSoloEseEstado()
    {
        var cliente = await ClienteComoAsync("admin@norte.test");
        var respuesta = await cliente.GetAsync("/api/v1/solicitudes?estado=Nueva&pageSize=100");

        Assert.Equal(HttpStatusCode.OK, respuesta.StatusCode);
        var listado = await respuesta.Content.ReadFromJsonAsync<ListadoApi>(JsonOpciones);
        Assert.NotEmpty(listado!.Items);
        Assert.All(listado.Items, i => Assert.Equal("Nueva", i.Estado));
    }

    [Fact]
    public async Task Solicitudes_BusquedaPorTexto_DevuelveCoincidencias()
    {
        var cliente = await ClienteComoAsync("admin@norte.test");
        var respuesta = await cliente.GetAsync("/api/v1/solicitudes?q=certificado&pageSize=100");

        Assert.Equal(HttpStatusCode.OK, respuesta.StatusCode);
        var listado = await respuesta.Content.ReadFromJsonAsync<ListadoApi>(JsonOpciones);
        Assert.NotEmpty(listado!.Items);
        Assert.All(listado.Items, i => Assert.Contains("certificado", i.Titulo, StringComparison.OrdinalIgnoreCase));
    }

    [Theory]
    [InlineData("page=0")]
    [InlineData("pageSize=101")]
    public async Task Solicitudes_PaginacionFueraDeRango_Devuelve400ParametroInvalido(string query)
    {
        var cliente = await ClienteComoAsync("admin@norte.test");
        var respuesta = await cliente.GetAsync($"/api/v1/solicitudes?{query}");

        Assert.Equal(HttpStatusCode.BadRequest, respuesta.StatusCode);
        Assert.Equal("application/problem+json", respuesta.Content.Headers.ContentType?.MediaType);
        var problema = await LeerProblemaAsync(respuesta);
        Assert.Equal("PARAMETRO_INVALIDO", problema.Codigo);
        Assert.NotNull(problema.Errores);
    }

    [Fact]
    public async Task Solicitudes_SortInvalido_Devuelve400ParametroInvalido()
    {
        var cliente = await ClienteComoAsync("admin@norte.test");
        var respuesta = await cliente.GetAsync("/api/v1/solicitudes?sort=invalido");

        Assert.Equal(HttpStatusCode.BadRequest, respuesta.StatusCode);
        var problema = await LeerProblemaAsync(respuesta);
        Assert.Equal("PARAMETRO_INVALIDO", problema.Codigo);
    }

    [Fact]
    public async Task Solicitudes_SolicitudDeOtraOrganizacion_Devuelve404NoEncontrada()
    {
        var cliente = await ClienteComoAsync("user1@sur.test");
        var respuesta = await cliente.GetAsync($"/api/v1/solicitudes/{IdSolicitudNorte1}");

        Assert.Equal(HttpStatusCode.NotFound, respuesta.StatusCode);
        var problema = await LeerProblemaAsync(respuesta);
        Assert.Equal("RECURSO_NO_ENCONTRADO", problema.Codigo);
    }

    [Fact]
    public async Task Solicitudes_SolicitanteVeSolicitudAjena_Devuelve403OperacionNoPermitida()
    {
        var cliente = await ClienteComoAsync("user2@norte.test");
        var respuesta = await cliente.GetAsync($"/api/v1/solicitudes/{IdSolicitudNorte1}");

        Assert.Equal(HttpStatusCode.Forbidden, respuesta.StatusCode);
        var problema = await LeerProblemaAsync(respuesta);
        Assert.Equal("OPERACION_NO_PERMITIDA", problema.Codigo);
    }

    [Fact]
    public async Task Solicitudes_Crear_Devuelve201ConUbicacionYDetalle()
    {
        var cliente = await ClienteComoAsync("user1@norte.test");
        var respuesta = await cliente.PostAsJsonAsync("/api/v1/solicitudes", new
        {
            titulo = "No puedo acceder al sistema",
            descripcion = "Al ingresar mis credenciales el sistema me devuelve a la pantalla de login.",
            categoriaId = SeedIds.CategoriaIncidenteNorte,
            prioridad = "Alta"
        });

        Assert.Equal(HttpStatusCode.Created, respuesta.StatusCode);
        var detalle = await respuesta.Content.ReadFromJsonAsync<SolicitudDetalleApi>(JsonOpciones);
        Assert.Matches("^SOL-2026-\\d{5}$", detalle!.Codigo);
        Assert.Equal("Nueva", detalle.Estado);
        Assert.Equal("No puedo acceder al sistema", detalle.Titulo);
        Assert.Equal(SeedIds.UsuarioUser1Norte, detalle.Solicitante.Id);
        Assert.Null(detalle.Agente);

        var ubicacion = respuesta.Headers.Location?.ToString();
        Assert.NotNull(ubicacion);
        Assert.Contains($"/api/v1/solicitudes/{detalle.Id}", ubicacion);

        var get = await cliente.GetAsync(ubicacion!);
        Assert.Equal(HttpStatusCode.OK, get.StatusCode);
    }

    [Fact]
    public async Task Solicitudes_Crear_ConCamposInvalidos_Devuelve422Validacion()
    {
        var cliente = await ClienteComoAsync("user1@norte.test");
        var respuesta = await cliente.PostAsJsonAsync("/api/v1/solicitudes", new
        {
            titulo = "abc",
            descripcion = "corta",
            categoriaId = SeedIds.CategoriaIncidenteNorte,
            prioridad = "Alta"
        });

        Assert.Equal(HttpStatusCode.UnprocessableEntity, respuesta.StatusCode);
        Assert.Equal("application/problem+json", respuesta.Content.Headers.ContentType?.MediaType);
        var problema = await LeerProblemaAsync(respuesta);
        Assert.Equal("VALIDACION", problema.Codigo);
        Assert.NotNull(problema.Errores);
        Assert.True(problema.Errores.ContainsKey("titulo") || problema.Errores.ContainsKey("descripcion"));
    }

    [Fact]
    public async Task Solicitudes_EditarPrioridad_RecalculaSlaSinTocarFechaCreacion()
    {
        var cliente = await ClienteComoAsync("user1@norte.test");
        var creada = await CrearSolicitudAsync(cliente, "Problema de conexion con la red", "Baja");

        var respuesta = await cliente.PutAsJsonAsync($"/api/v1/solicitudes/{creada.Id}", new
        {
            titulo = creada.Titulo,
            descripcion = creada.Descripcion,
            categoriaId = SeedIds.CategoriaIncidenteNorte,
            prioridad = "Critica"
        });

        Assert.Equal(HttpStatusCode.OK, respuesta.StatusCode);
        var editada = await respuesta.Content.ReadFromJsonAsync<SolicitudDetalleApi>(JsonOpciones);
        Assert.Equal("Critica", editada!.Prioridad);
        Assert.Equal(creada.FechaCreacion, editada.FechaCreacion);
        Assert.Equal(creada.FechaCreacion.AddHours(4), editada.FechaLimiteSla);
    }

    [Fact]
    public async Task Solicitudes_FlujoCompletoAsignarIniciarResolverCerrar()
    {
        var solicitante = await ClienteComoAsync("user1@norte.test");
        var creada = await CrearSolicitudAsync(solicitante, "Solicitud para recorrer el flujo completo", "Media");
        var agente = await ClienteComoAsync("agente1@norte.test");

        var asignar = await agente.PostAsJsonAsync(
            $"/api/v1/solicitudes/{creada.Id}/transiciones", new { accion = "asignar", agenteId = SeedIds.UsuarioAgente2Norte });
        Assert.Equal(HttpStatusCode.OK, asignar.StatusCode);
        var asignada = await asignar.Content.ReadFromJsonAsync<SolicitudDetalleApi>(JsonOpciones);
        Assert.Equal("Asignada", asignada!.Estado);
        Assert.Equal(SeedIds.UsuarioAgente2Norte, asignada.Agente!.Id);

        var iniciar = await agente.PostAsJsonAsync(
            $"/api/v1/solicitudes/{creada.Id}/transiciones", new { accion = "iniciar" });
        var enProceso = await iniciar.Content.ReadFromJsonAsync<SolicitudDetalleApi>(JsonOpciones);
        Assert.Equal("EnProceso", enProceso!.Estado);

        var resolver = await agente.PostAsJsonAsync(
            $"/api/v1/solicitudes/{creada.Id}/transiciones", new { accion = "resolver", motivo = "Se restablecio el servicio y se valido el acceso del usuario." });
        var resuelta = await resolver.Content.ReadFromJsonAsync<SolicitudDetalleApi>(JsonOpciones);
        Assert.Equal("Resuelta", resuelta!.Estado);
        Assert.NotNull(resuelta.MotivoResolucion);

        var cerrar = await agente.PostAsJsonAsync(
            $"/api/v1/solicitudes/{creada.Id}/transiciones", new { accion = "cerrar" });
        var cerrada = await cerrar.Content.ReadFromJsonAsync<SolicitudDetalleApi>(JsonOpciones);
        Assert.Equal("Cerrada", cerrada!.Estado);
    }

    [Fact]
    public async Task Solicitudes_TransicionInvalida_Devuelve409TransicionInvalida()
    {
        var solicitante = await ClienteComoAsync("user1@norte.test");
        var creada = await CrearSolicitudAsync(solicitante, "Solicitud para transicion invalida", "Alta");
        var agente = await ClienteComoAsync("agente1@norte.test");

        var respuesta = await agente.PostAsJsonAsync(
            $"/api/v1/solicitudes/{creada.Id}/transiciones", new { accion = "resolver", motivo = "Motivo de resolucion con mas de veinte caracteres." });

        Assert.Equal(HttpStatusCode.Conflict, respuesta.StatusCode);
        Assert.Equal("application/problem+json", respuesta.Content.Headers.ContentType?.MediaType);
        var problema = await LeerProblemaAsync(respuesta);
        Assert.Equal("TRANSICION_INVALIDA", problema.Codigo);
    }

    [Fact]
    public async Task Solicitudes_ResolverConMotivoCorto_Devuelve422MotivoRequerido()
    {
        var solicitante = await ClienteComoAsync("user1@norte.test");
        var creada = await CrearSolicitudAsync(solicitante, "Solicitud para validar motivo requerido", "Media");
        var agente = await ClienteComoAsync("agente1@norte.test");

        await agente.PostAsJsonAsync(
            $"/api/v1/solicitudes/{creada.Id}/transiciones", new { accion = "asignar", agenteId = SeedIds.UsuarioAgente1Norte });
        await agente.PostAsJsonAsync(
            $"/api/v1/solicitudes/{creada.Id}/transiciones", new { accion = "iniciar" });

        var respuesta = await agente.PostAsJsonAsync(
            $"/api/v1/solicitudes/{creada.Id}/transiciones", new { accion = "resolver", motivo = "Corto" });

        Assert.Equal(HttpStatusCode.UnprocessableEntity, respuesta.StatusCode);
        var problema = await LeerProblemaAsync(respuesta);
        Assert.Equal("MOTIVO_REQUERIDO", problema.Codigo);
    }

    [Fact]
    public async Task Solicitudes_AsignarSolicitanteComoAgente_Devuelve422AgenteInvalido()
    {
        var solicitante = await ClienteComoAsync("user1@norte.test");
        var creada = await CrearSolicitudAsync(solicitante, "Solicitud para validar agente invalido", "Media");
        var agente = await ClienteComoAsync("agente1@norte.test");

        var respuesta = await agente.PostAsJsonAsync(
            $"/api/v1/solicitudes/{creada.Id}/transiciones", new { accion = "asignar", agenteId = SeedIds.UsuarioUser1Norte });

        Assert.Equal(HttpStatusCode.UnprocessableEntity, respuesta.StatusCode);
        var problema = await LeerProblemaAsync(respuesta);
        Assert.Equal("AGENTE_INVALIDO", problema.Codigo);
    }

    [Fact]
    public async Task Solicitudes_CancelarConMotivoCorto_Devuelve422MotivoRequerido()
    {
        var solicitante = await ClienteComoAsync("user1@norte.test");
        var creada = await CrearSolicitudAsync(solicitante, "Solicitud para cancelar", "Baja");
        var admin = await ClienteComoAsync("admin@norte.test");

        var respuesta = await admin.PostAsJsonAsync(
            $"/api/v1/solicitudes/{creada.Id}/transiciones", new { accion = "cancelar", motivo = "Corto" });

        Assert.Equal(HttpStatusCode.UnprocessableEntity, respuesta.StatusCode);
        var problema = await LeerProblemaAsync(respuesta);
        Assert.Equal("MOTIVO_REQUERIDO", problema.Codigo);
    }
}
