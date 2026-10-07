using System.ComponentModel.DataAnnotations;

namespace ApiClinica.DTOs;

public class ConsultaCreateDTO
{
    [Required] public int? PacienteId { get; set; }
    [Required] public int? MedicoId { get; set; }
    [Required] public DateTime? DataHora { get; set; }
}
