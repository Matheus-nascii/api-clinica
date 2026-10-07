using ApiClinica.DTOs;
using ApiClinica.Models;

namespace ApiClinica.Mappers;

public static class MedicoMapper
{
    public static MedicoReadDTO ToReadDTO(Medico m) => new()
    {
        Id = m.Id,
        Nome = m.Nome,
        Email = m.Email,
        Telefone = m.Telefone,
        CRM = m.CRM
    };

    public static Medico ToEntity(MedicoCreateDTO dto) => new()
    {
        Nome = dto.Nome.Trim(),
        Email = dto.Email.Trim(),
        Telefone = dto.Telefone.Trim(),
        CRM = dto.CRM.Trim()
    };

    public static void ApplyUpdate(Medico m, MedicoUpdateDTO dto)
    {
        if (dto.Nome != null) m.Nome = dto.Nome.Trim();
        if (dto.Email != null) m.Email = dto.Email.Trim();
        if (dto.Telefone != null) m.Telefone = dto.Telefone.Trim();
        if (dto.CRM != null) m.CRM = dto.CRM.Trim();
    }
}
