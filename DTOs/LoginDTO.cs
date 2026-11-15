using System.ComponentModel.DataAnnotations;

namespace ApiClinica.DTOs;

// Contrato publico de credenciais enviado ao endpoint de login.
public class LoginDTO
{
    [Required, EmailAddress] public string Email { get; set; } = string.Empty;
    [Required] public string Senha { get; set; } = string.Empty;
}
