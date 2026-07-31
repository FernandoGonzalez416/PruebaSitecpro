using MesaSitec.Aplicacion.Autenticacion;

namespace MesaSitec.Infraestructura.Autenticacion;

public class VerificadorPasswordBcrypt : IVerificadorPassword
{
    public bool Verificar(string password, string hash) =>
        BCrypt.Net.BCrypt.Verify(password, hash);
}
