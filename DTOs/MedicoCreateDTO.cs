using System.ComponentModel.DataAnnotations;

namespace ApiClinica.DTOs;

public class MedicoCreateDTO
{
    [Required] public string Nome { get; set; } = string.Empty;
    [Required] public string Email { get; set; } = string.Empty;
    [Required] public string Telefone { get; set; } = string.Empty;
    [Required] public string CRM { get; set; } = string.Empty;
}
