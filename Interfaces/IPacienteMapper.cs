using ApiClinica.DTOs;
using ApiClinica.Models;

namespace ApiClinica.Interfaces;

// Contrato da conversao entre a entidade Paciente e seus DTOs; implementado por PacienteMapper.
public interface IPacienteMapper
{
    PacienteReadDTO ToReadDTO(Paciente paciente);
    Paciente ToEntity(PacienteCreateDTO dto);
    void ApplyUpdate(Paciente paciente, PacienteUpdateDTO dto);
}
