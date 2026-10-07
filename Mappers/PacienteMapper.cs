using ApiClinica.DTOs;
using ApiClinica.Models;
using ApiClinica.Validations;

namespace ApiClinica.Mappers;

// Mapeamento MANUAL entre Model e DTOs (sem AutoMapper)
public static class PacienteMapper
{
    // Model -> DTO de leitura
    public static PacienteReadDTO ToReadDTO(Paciente p) => new()
    {
        Id = p.Id,
        Nome = p.Nome,
        Email = p.Email,
        Telefone = p.Telefone,
        DataNasc = p.DataNasc,
        Cpf = Validador.FormatarCpf(p.Cpf)
    };

    // DTO de criação -> Model
    public static Paciente ToEntity(PacienteCreateDTO dto) => new()
    {
        Nome = dto.Nome.Trim(),
        Email = dto.Email.Trim(),
        Telefone = dto.Telefone.Trim(),
        DataNasc = dto.DataNasc!.Value.Date,
        Cpf = Validador.SomenteNumeros(dto.Cpf)
    };

    // PATCH: só altera os campos que vieram diferentes de null
    public static void ApplyUpdate(Paciente p, PacienteUpdateDTO dto)
    {
        if (dto.Nome != null) p.Nome = dto.Nome.Trim();
        if (dto.Email != null) p.Email = dto.Email.Trim();
        if (dto.Telefone != null) p.Telefone = dto.Telefone.Trim();
        if (dto.DataNasc != null) p.DataNasc = dto.DataNasc.Value.Date;
        // CPF nunca é alterado aqui
    }
}
