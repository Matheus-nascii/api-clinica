using ApiClinica.DTOs;
using ApiClinica.Models;

namespace ApiClinica.Interfaces;

// Contrato da conversao entre a entidade Medico e seus DTOs; implementado por MedicoMapper.
public interface IMedicoMapper
{
    MedicoReadDTO ToReadDTO(Medico medico);
    Medico ToEntity(MedicoCreateDTO dto);
    void ApplyUpdate(Medico medico, MedicoUpdateDTO dto);
}
