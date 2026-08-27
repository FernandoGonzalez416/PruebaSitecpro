namespace HelpDesk.Aplicacion.Autenticacion.DTOs;

public class LoginResponse
{
    public string AccessToken { get; set; } = null!;
    public int ExpiraEn { get; set; } = 28800;
    public UsuarioDto Usuario { get; set; } = null!;
}
