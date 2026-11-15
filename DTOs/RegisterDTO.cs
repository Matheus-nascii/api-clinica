using System.ComponentModel.DataAnnotations;

namespace ApiClinica.DTOs;

// Contrato publico para cadastro de usuario comum; nao aceita Role para impedir autoelevacao a Admin.
public class RegisterDTO
{
    [Required, MinLength(2)] public string Nome { get; set; } = string.Empty;
    [Required, EmailAddress] public string Email { get; set; } = string.Empty;
    [Required, MinLength(8)] public string Senha { get; set; } = string.Empty;
}
