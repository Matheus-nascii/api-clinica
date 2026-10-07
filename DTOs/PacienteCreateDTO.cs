using System.ComponentModel.DataAnnotations;

namespace ApiClinica.DTOs;

// Dados que o cliente envia para CRIAR um paciente (POST)
public class PacienteCreateDTO
{
    [Required] public string Nome { get; set; } = string.Empty;
    [Required] public string Email { get; set; } = string.Empty;
    [Required] public string Telefone { get; set; } = string.Empty;
    [Required] public DateTime? DataNasc { get; set; }
    [Required] public string Cpf { get; set; } = string.Empty;
}
