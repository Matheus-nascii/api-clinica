using ApiClinica.Data;
using ApiClinica.DTOs;
using ApiClinica.Interfaces;
using ApiClinica.Models;
using Microsoft.EntityFrameworkCore;

namespace ApiClinica.Services;

// Implementa o agendamento e concentra as regras de data, vinculos e conflitos de horario.
public class ConsultaService(AppDbContext context, IConsultaMapper mapper) : IConsultaService
{
    public async Task<IEnumerable<ConsultaReadDTO>> GetAllAsync() =>
        // Include traz os nomes relacionados para o DTO; a ordenação mostra primeiro a consulta mais próxima.
        (await context.Consultas.Include(c => c.Paciente).Include(c => c.Medico).AsNoTracking().OrderBy(c => c.DataHora).ToListAsync()).Select(mapper.ToReadDTO);

    public async Task<ServiceResult<ConsultaReadDTO>> GetByIdAsync(int id)
    {
        var consulta = await BuscarComNomesAsync(id);
        return consulta is null ? ServiceResult<ConsultaReadDTO>.Fail(ServiceErrorType.NotFound, $"Consulta {id} não encontrada.") : ServiceResult<ConsultaReadDTO>.Ok(mapper.ToReadDTO(consulta));
    }

    public async Task<ServiceResult<ConsultaReadDTO>> CreateAsync(ConsultaCreateDTO dto)
    {
        // Converte os IDs e a data enviados pelo cliente para a entidade que será validada.
        var consulta = mapper.ToEntity(dto);
        var errors = await ValidarAsync(consulta);
        if (errors.Count > 0) return ServiceResult<ConsultaReadDTO>.Fail(ServiceErrorType.Validation, errors.ToArray());
        // Só após todas as regras passarem, a consulta é marcada para INSERT no SQLite.
        context.Consultas.Add(consulta);
        await context.SaveChangesAsync();
        // O INSERT não preenche navegações; carrega paciente/médico para devolver seus nomes no DTO.
        await CarregarNomesAsync(consulta);
        return ServiceResult<ConsultaReadDTO>.Ok(mapper.ToReadDTO(consulta));
    }

    public async Task<ServiceResult<ConsultaReadDTO>> UpdateAsync(int id, ConsultaUpdateDTO dto)
    {
        var consulta = await context.Consultas.FindAsync(id);
        if (consulta is null) return ServiceResult<ConsultaReadDTO>.Fail(ServiceErrorType.NotFound, $"Consulta {id} não encontrada.");
        // Aplica o PATCH em memória antes de validar o resultado completo do novo agendamento.
        mapper.ApplyUpdate(consulta, dto);
        // Revalida data, vínculos e conflitos inclusive quando somente paciente ou médico mudou.
        var errors = await ValidarAsync(consulta);
        if (errors.Count > 0) return ServiceResult<ConsultaReadDTO>.Fail(ServiceErrorType.Validation, errors.ToArray());
        await context.SaveChangesAsync();
        await CarregarNomesAsync(consulta);
        return ServiceResult<ConsultaReadDTO>.Ok(mapper.ToReadDTO(consulta));
    }

    public async Task<ServiceResult<bool>> DeleteAsync(int id)
    {
        var consulta = await context.Consultas.FindAsync(id);
        if (consulta is null) return ServiceResult<bool>.Fail(ServiceErrorType.NotFound, $"Consulta {id} não encontrada.");
        // Marca a consulta localizada para DELETE e confirma a exclusão no SQLite.
        context.Consultas.Remove(consulta);
        await context.SaveChangesAsync();
        return ServiceResult<bool>.Ok(true);
    }

    private async Task<List<string>> ValidarAsync(Consulta consulta)
    {
        var errors = new List<string>();
        // IDs menores/iguais a zero ou inexistentes não podem formar chaves estrangeiras válidas.
        if (consulta.PacienteId <= 0 || !await context.Pacientes.AnyAsync(p => p.Id == consulta.PacienteId)) errors.Add($"Paciente {consulta.PacienteId} não existe.");
        if (consulta.MedicoId <= 0 || !await context.Medicos.AnyAsync(m => m.Id == consulta.MedicoId)) errors.Add($"Médico {consulta.MedicoId} não existe.");
        // A data deve ser futura no momento da requisição.
        if (consulta.DataHora < DateTime.Now) errors.Add("Não é possível agendar consulta no passado.");
        // Sem paciente, médico ou data válida, não faz consultas de conflito desnecessárias.
        if (errors.Count > 0) return errors;

        // Uma consulta ocupa 30 min; estes limites detectam inícios dentro dessa janela.
        var limiteInicio = consulta.DataHora.AddMinutes(-Consulta.DuracaoMinutos);
        var limiteFim = consulta.DataHora.AddMinutes(Consulta.DuracaoMinutos);
        // Ignora a própria consulta no PATCH e procura horário que conflite para o mesmo médico.
        var conflitoMedico = await context.Consultas.FirstOrDefaultAsync(c => c.Id != consulta.Id && c.MedicoId == consulta.MedicoId && c.DataHora > limiteInicio && c.DataHora < limiteFim);
        if (conflitoMedico is not null) errors.Add(conflitoMedico.DataHora == consulta.DataHora ? "O médico já possui uma consulta neste mesmo horário." : $"Sobreposição de horário: o médico já tem consulta às {conflitoMedico.DataHora:HH:mm}.");
        // Aplica a mesma regra ao paciente para evitar dois atendimentos simultâneos.
        var conflitoPaciente = await context.Consultas.FirstOrDefaultAsync(c => c.Id != consulta.Id && c.PacienteId == consulta.PacienteId && c.DataHora > limiteInicio && c.DataHora < limiteFim);
        if (conflitoPaciente is not null) errors.Add(conflitoPaciente.DataHora == consulta.DataHora ? "O paciente já possui uma consulta neste mesmo horário." : $"Sobreposição de horário: o paciente já tem consulta às {conflitoPaciente.DataHora:HH:mm}.");
        return errors;
    }

    // GET por ID inclui as navegações porque ConsultaReadDTO também mostra NomePaciente e NomeMedico.
    private Task<Consulta?> BuscarComNomesAsync(int id) => context.Consultas.Include(c => c.Paciente).Include(c => c.Medico).AsNoTracking().FirstOrDefaultAsync(c => c.Id == id);

    private async Task CarregarNomesAsync(Consulta consulta)
    {
        // Carrega explicitamente as navegações após INSERT/PATCH para montar a resposta completa.
        await context.Entry(consulta).Reference(c => c.Paciente).LoadAsync();
        await context.Entry(consulta).Reference(c => c.Medico).LoadAsync();
    }
}
