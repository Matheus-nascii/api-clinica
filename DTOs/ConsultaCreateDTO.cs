using System.ComponentModel.DataAnnotations;

namespace ApiClinica.DTOs;

// Contrato do corpo recebido no POST para solicitar um novo agendamento.
public class ConsultaCreateDTO
{
    [Required] public int? PacienteId { get; set; }
    [Required] public int? MedicoId { get; set; }
    [Required] public DateTime? DataHora { get; set; }
}
