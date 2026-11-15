using ApiClinica.DTOs;
using ApiClinica.Interfaces;
using ApiClinica.Models;

namespace ApiClinica.Mappers;

// Mapeamento manual entre entidade Consulta e DTOs, incluindo dados calculados para a resposta.
public class ConsultaMapper : IConsultaMapper
{
    // As navegações já carregadas pelo service fornecem os nomes exibidos no retorno da consulta.
    public ConsultaReadDTO ToReadDTO(Consulta c) => new()
    {
        Id = c.Id,
        PacienteId = c.PacienteId,
        NomePaciente = c.Paciente?.Nome,
        MedicoId = c.MedicoId,
        NomeMedico = c.Medico?.Nome,
        DataHora = c.DataHora,
        DataHoraFim = c.DataHora.AddMinutes(Consulta.DuracaoMinutos)
    };

    // Required é validado pelo ASP.NET antes desta chamada; por isso os valores obrigatórios existem aqui.
    public Consulta ToEntity(ConsultaCreateDTO dto) => new()
    {
        PacienteId = dto.PacienteId!.Value,
        MedicoId = dto.MedicoId!.Value,
        DataHora = dto.DataHora!.Value
    };

    public void ApplyUpdate(Consulta c, ConsultaUpdateDTO dto)
    {
        // Atualiza somente IDs/data presentes no PATCH para que a validação reavalie o estado final.
        if (dto.PacienteId != null) c.PacienteId = dto.PacienteId.Value;
        if (dto.MedicoId != null) c.MedicoId = dto.MedicoId.Value;
        if (dto.DataHora != null) c.DataHora = dto.DataHora.Value;
    }
}
