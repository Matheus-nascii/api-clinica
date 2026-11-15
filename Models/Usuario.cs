namespace ApiClinica.Models;

// Entidade de autenticacao: armazena identidade, hash da senha e papel de autorizacao.
public class Usuario
{
    public int Id { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string SenhaHash { get; set; } = string.Empty;
    public string Role { get; set; } = Roles.Usuario;
}

// Papeis reconhecidos pelas politicas [Authorize] e gravados como claim no JWT.
public static class Roles
{
    public const string Usuario = "Usuario";
    public const string Admin = "Admin";
}
