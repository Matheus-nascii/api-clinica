using ApiClinica.DTOs;
using ApiClinica.Models;

namespace ApiClinica.Mappers;

public static class ConsultaMapper
{
    public static ConsultaReadDTO ToReadDTO(Consulta c) => new()
    {
        Id = c.Id,
        PacienteId = c.PacienteId,
        NomePaciente = c.Paciente?.Nome,
        MedicoId = c.MedicoId,
        NomeMedico = c.Medico?.Nome,
        DataHora = c.DataHora,
        DataHoraFim = c.DataHora.AddMinutes(Consulta.DuracaoMinutos)
    };

    public static Consulta ToEntity(ConsultaCreateDTO dto) => new()
    {
        PacienteId = dto.PacienteId!.Value,
        MedicoId = dto.MedicoId!.Value,
        DataHora = dto.DataHora!.Value
    };

    public static void ApplyUpdate(Consulta c, ConsultaUpdateDTO dto)
    {
        if (dto.PacienteId != null) c.PacienteId = dto.PacienteId.Value;
        if (dto.MedicoId != null) c.MedicoId = dto.MedicoId.Value;
        if (dto.DataHora != null) c.DataHora = dto.DataHora.Value;
    }
}
