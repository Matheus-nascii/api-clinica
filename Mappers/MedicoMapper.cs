using ApiClinica.DTOs;
using ApiClinica.Interfaces;
using ApiClinica.Models;

namespace ApiClinica.Mappers;

// Mapeamento manual entre entidade Medico e seus DTOs de entrada e saida.
public class MedicoMapper : IMedicoMapper
{
    // Copia somente os dados públicos que a API devolve ao cliente.
    public MedicoReadDTO ToReadDTO(Medico m) => new()
    {
        Id = m.Id,
        Nome = m.Nome,
        Email = m.Email,
        Telefone = m.Telefone,
        CRM = m.CRM
    };

    // Normaliza espaços nos dados recebidos antes de persistir o novo médico.
    public Medico ToEntity(MedicoCreateDTO dto) => new()
    {
        Nome = dto.Nome.Trim(),
        Email = dto.Email.Trim(),
        Telefone = dto.Telefone.Trim(),
        CRM = dto.CRM.Trim()
    };

    public void ApplyUpdate(Medico m, MedicoUpdateDTO dto)
    {
        // Null representa campo ausente no PATCH e, por isso, não sobrescreve o valor salvo.
        if (dto.Nome != null) m.Nome = dto.Nome.Trim();
        if (dto.Email != null) m.Email = dto.Email.Trim();
        if (dto.Telefone != null) m.Telefone = dto.Telefone.Trim();
        if (dto.CRM != null) m.CRM = dto.CRM.Trim();
    }
}
