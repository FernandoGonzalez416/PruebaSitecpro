namespace HelpDesk.Aplicacion.Autenticacion;

public interface IVerificadorPassword
{
    bool Verificar(string password, string hash);
}
