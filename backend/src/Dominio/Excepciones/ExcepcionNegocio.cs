namespace MesaSitec.Dominio.Excepciones;

public class ExcepcionNegocio : Exception
{
    public string Codigo { get; }
    public int Status { get; }
    public string Detail { get; }
    public Dictionary<string, string[]>? Errores { get; }

    public ExcepcionNegocio(
        string codigo,
        string message,
        int status = 400,
        string? detail = null,
        Dictionary<string, string[]>? errores = null)
        : base(message)
    {
        Codigo = codigo;
        Status = status;
        Detail = detail ?? message;
        Errores = errores;
    }
}
