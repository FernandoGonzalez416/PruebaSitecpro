using MesaSitec.Dominio.Excepciones;
using Microsoft.AspNetCore.Diagnostics;

namespace MesaSitec.Api.Errores;

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
                break;
            default:
                _logger.LogError(exception, "Excepción no controlada en la API");
                break;
        }

        httpContext.Response.StatusCode = status;
        await httpContext.Response.WriteAsJsonAsync(
            new
            {
                type = $"https://mesasitec.local/errores/{CodigoAKebab(codigo)}",
                title,
                status,
                detail,
                codigo
            },
            options: null,
            contentType: "application/problem+json",
            cancellationToken);

        return true;
    }

    private static string CodigoAKebab(string codigo) =>
        codigo.ToLowerInvariant().Replace('_', '-');
}
