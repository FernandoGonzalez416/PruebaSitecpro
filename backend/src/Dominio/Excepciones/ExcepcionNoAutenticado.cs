namespace HelpDesk.Dominio.Excepciones;

public class ExcepcionNoAutenticado : ExcepcionNegocio
{
    public ExcepcionNoAutenticado(string message = "Credenciales inválidas.")
        : base(codigo: "NO_AUTENTICADO", message: message, status: 401, detail: message)
    {
    }
}
