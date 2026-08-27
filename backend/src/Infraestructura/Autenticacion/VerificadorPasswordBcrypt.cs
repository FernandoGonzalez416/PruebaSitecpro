using HelpDesk.Aplicacion.Autenticacion;

namespace HelpDesk.Infraestructura.Autenticacion;

public class VerificadorPasswordBcrypt : IVerificadorPassword
{
    public bool Verificar(string password, string hash) =>
        BCrypt.Net.BCrypt.Verify(password, hash);
}
