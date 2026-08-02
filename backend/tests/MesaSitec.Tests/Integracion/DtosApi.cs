using System.Text.Json.Serialization;

namespace MesaSitec.Tests.Integracion;

internal sealed record Problema(
    [property: JsonPropertyName("type")] string? Type,
    [property: JsonPropertyName("title")] string? Title,
    [property: JsonPropertyName("status")] int Status,
    [property: JsonPropertyName("detail")] string? Detail,
    [property: JsonPropertyName("codigo")] string Codigo,
    [property: JsonPropertyName("errores")] Dictionary<string, string[]>? Errores);

internal sealed record UsuarioApi(
    [property: JsonPropertyName("id")] Guid Id,
    [property: JsonPropertyName("nombre")] string Nombre,
    [property: JsonPropertyName("email")] string Email,
    [property: JsonPropertyName("rol")] string Rol,
    [property: JsonPropertyName("tenantId")] Guid TenantId,
    [property: JsonPropertyName("tenantNombre")] string TenantNombre);

internal sealed record LoginResponseApi(
    [property: JsonPropertyName("accessToken")] string AccessToken,
    [property: JsonPropertyName("expiraEn")] int ExpiraEn,
    [property: JsonPropertyName("usuario")] UsuarioApi Usuario);

internal sealed record CategoriaApi(
    [property: JsonPropertyName("id")] Guid Id,
    [property: JsonPropertyName("nombre")] string Nombre,
    [property: JsonPropertyName("slaHoras")] int SlaHoras);

internal sealed record CategoriaResumenApi(
    [property: JsonPropertyName("id")] Guid Id,
    [property: JsonPropertyName("nombre")] string Nombre);

internal sealed record UsuarioResumenApi(
    [property: JsonPropertyName("id")] Guid Id,
    [property: JsonPropertyName("nombre")] string Nombre);

internal sealed record SolicitudListaItemApi(
    [property: JsonPropertyName("id")] Guid Id,
    [property: JsonPropertyName("codigo")] string Codigo,
    [property: JsonPropertyName("titulo")] string Titulo,
    [property: JsonPropertyName("estado")] string Estado,
    [property: JsonPropertyName("prioridad")] string Prioridad,
    [property: JsonPropertyName("categoria")] CategoriaResumenApi Categoria,
    [property: JsonPropertyName("fechaCreacion")] DateTime FechaCreacion,
    [property: JsonPropertyName("fechaLimiteSla")] DateTime FechaLimiteSla,
    [property: JsonPropertyName("vencida")] bool Vencida);

internal sealed record ListadoApi(
    [property: JsonPropertyName("items")] IReadOnlyList<SolicitudListaItemApi> Items,
    [property: JsonPropertyName("page")] int Page,
    [property: JsonPropertyName("pageSize")] int PageSize,
    [property: JsonPropertyName("total")] int Total,
    [property: JsonPropertyName("totalPaginas")] int TotalPaginas);

internal sealed record SolicitudDetalleApi(
    [property: JsonPropertyName("id")] Guid Id,
    [property: JsonPropertyName("codigo")] string Codigo,
    [property: JsonPropertyName("titulo")] string Titulo,
    [property: JsonPropertyName("descripcion")] string Descripcion,
    [property: JsonPropertyName("estado")] string Estado,
    [property: JsonPropertyName("prioridad")] string Prioridad,
    [property: JsonPropertyName("categoria")] CategoriaResumenApi Categoria,
    [property: JsonPropertyName("solicitante")] UsuarioResumenApi Solicitante,
    [property: JsonPropertyName("agente")] UsuarioResumenApi? Agente,
    [property: JsonPropertyName("fechaCreacion")] DateTime FechaCreacion,
    [property: JsonPropertyName("fechaLimiteSla")] DateTime FechaLimiteSla,
    [property: JsonPropertyName("motivoResolucion")] string? MotivoResolucion,
    [property: JsonPropertyName("motivoCancelacion")] string? MotivoCancelacion,
    [property: JsonPropertyName("vencida")] bool Vencida);

internal sealed record HealthApi(
    [property: JsonPropertyName("estado")] string Estado);
