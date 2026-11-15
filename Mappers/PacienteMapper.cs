using ApiClinica.DTOs;
using ApiClinica.Interfaces;
using ApiClinica.Models;
using ApiClinica.Validations;

namespace ApiClinica.Mappers;

// Mapeamento manual entre entidade Paciente e seus DTOs, sem expor detalhes de persistencia.
public class PacienteMapper : IPacienteMapper
{
    // Model -> DTO de leitura: formata o CPF para exibição, embora ele seja salvo só com números.
    public PacienteReadDTO ToReadDTO(Paciente p) => new()
    {
        Id = p.Id,
        Nome = p.Nome,
        Email = p.Email,
        Telefone = p.Telefone,
        DataNasc = p.DataNasc,
        Cpf = Validador.FormatarCpf(p.Cpf)
    };

    // DTO de criação -> Model: remove espaços e normaliza a data/CPF antes do INSERT.
    public Paciente ToEntity(PacienteCreateDTO dto) => new()
    {
        Nome = dto.Nome.Trim(),
        Email = dto.Email.Trim(),
        Telefone = dto.Telefone.Trim(),
        DataNasc = dto.DataNasc!.Value.Date,
        Cpf = Validador.SomenteNumeros(dto.Cpf)
    };

    // PATCH: só altera os campos que vieram diferentes de null
    public void ApplyUpdate(Paciente p, PacienteUpdateDTO dto)
    {
        // Cada if interpreta null como "campo não enviado", preservando o valor atual no PATCH.
        if (dto.Nome != null) p.Nome = dto.Nome.Trim();
        if (dto.Email != null) p.Email = dto.Email.Trim();
        if (dto.Telefone != null) p.Telefone = dto.Telefone.Trim();
        if (dto.DataNasc != null) p.DataNasc = dto.DataNasc.Value.Date;
        // CPF nunca é alterado aqui
    }
}
