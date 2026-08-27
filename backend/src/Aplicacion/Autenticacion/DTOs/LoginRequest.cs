namespace HelpDesk.Aplicacion.Autenticacion.DTOs;

public class LoginRequest
{
    public string Email { get; set; } = null!;
    public string Password { get; set; } = null!;
}
