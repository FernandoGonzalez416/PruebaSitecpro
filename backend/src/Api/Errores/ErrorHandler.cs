using HelpDesk.Dominio.Excepciones;
using Microsoft.AspNetCore.Diagnostics;

namespace HelpDesk.Api.Errores;

public class ErrorHandler : IExceptionHandler
{
    private readonly ILogger<ErrorHandler> _logger;

    public ErrorHandler(ILogger<ErrorHandler> logger)
    {
        _logger = logger;
    }

    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        var codigo = "ERROR_INTERNO";
        var status = StatusCodes.Status500InternalServerError;
        var title = "Error interno del servidor";
        var detail = "Ocurrió un error inesperado.";
        Dictionary<string, string[]>? errores = null;

        switch (exception)
        {
            case ExcepcionNoAutenticado ex:
                codigo = ex.Codigo;
                status = ex.Status;
                title = ex.Message;
                detail = ex.Detail;
                break;
            case ExcepcionNegocio ex:
                codigo = ex.Codigo;
                status = ex.Status;
                title = ex.Message;
                detail = ex.Detail;
                errores = ex.Errores;
                break;
            default:
                _logger.LogError(exception, "Excepción no controlada en la API");
                break;
        }

        var cuerpo = new Dictionary<string, object?>
        {
            ["type"] = $"https://helpdesk.local/errores/{CodigoAKebab(codigo)}",
            ["title"] = title,
            ["status"] = status,
            ["detail"] = detail,
            ["codigo"] = codigo
        };
        if (errores is { Count: > 0 })
        {
            cuerpo["errores"] = errores;
        }

        httpContext.Response.StatusCode = status;
        await httpContext.Response.WriteAsJsonAsync(
            cuerpo,
            options: null,
            contentType: "application/problem+json",
            cancellationToken);

        return true;
    }

    private static string CodigoAKebab(string codigo) =>
        codigo.ToLowerInvariant().Replace('_', '-');
}
