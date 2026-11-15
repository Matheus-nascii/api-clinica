using System.ComponentModel.DataAnnotations;

namespace ApiClinica.DTOs;

// Contrato do corpo recebido no POST de medicos; Required e validado pelo ASP.NET antes do service.
public class MedicoCreateDTO
{
    [Required] public string Nome { get; set; } = string.Empty;
    [Required] public string Email { get; set; } = string.Empty;
    [Required] public string Telefone { get; set; } = string.Empty;
    [Required] public string CRM { get; set; } = string.Empty;
}
