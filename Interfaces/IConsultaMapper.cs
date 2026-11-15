using ApiClinica.DTOs;
using ApiClinica.Models;

namespace ApiClinica.Interfaces;

// Contrato da conversao entre a entidade Consulta e seus DTOs; implementado por ConsultaMapper.
public interface IConsultaMapper
{
    ConsultaReadDTO ToReadDTO(Consulta consulta);
    Consulta ToEntity(ConsultaCreateDTO dto);
    void ApplyUpdate(Consulta consulta, ConsultaUpdateDTO dto);
}
