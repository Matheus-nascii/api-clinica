using ApiClinica.Data;
using ApiClinica.DTOs;
using ApiClinica.Interfaces;
using ApiClinica.Validations;
using Microsoft.EntityFrameworkCore;

namespace ApiClinica.Services;

// Implementa os casos de uso e as regras de negocio do paciente.
public class PacienteService(AppDbContext context, IPacienteMapper mapper) : IPacienteService
{
    public async Task<IEnumerable<PacienteReadDTO>> GetAllAsync() =>
        // AsNoTracking é leitura sem rastreamento: reduz custo porque esta listagem não será alterada.
        (await context.Pacientes.AsNoTracking().ToListAsync()).Select(mapper.ToReadDTO);

    public async Task<ServiceResult<PacienteReadDTO>> GetByIdAsync(int id)
    {
        // Busca um registro pelo ID; null gera 404 no controller em vez de devolver um DTO vazio.
        var paciente = await context.Pacientes.AsNoTracking().FirstOrDefaultAsync(p => p.Id == id);
        return paciente is null
            ? ServiceResult<PacienteReadDTO>.Fail(ServiceErrorType.NotFound, $"Paciente {id} não encontrado.")
            : ServiceResult<PacienteReadDTO>.Ok(mapper.ToReadDTO(paciente));
    }

    public async Task<ServiceResult<PacienteReadDTO>> CreateAsync(PacienteCreateDTO dto)
    {
        // Valida todos os campos antes de inserir, para devolver a lista completa de erros ao cliente.
        var errors = Validar(dto);
        if (errors.Count > 0) return ServiceResult<PacienteReadDTO>.Fail(ServiceErrorType.Validation, errors.ToArray());

        // O CPF é comparado sem pontuação, igual ao formato persistido pelo mapper.
        var cpf = Validador.SomenteNumeros(dto.Cpf);
        if (await context.Pacientes.AnyAsync(p => p.Cpf == cpf))
            return ServiceResult<PacienteReadDTO>.Fail(ServiceErrorType.Conflict, "Já existe um paciente cadastrado com este CPF.");

        var paciente = mapper.ToEntity(dto);
        // Add marca a entidade para INSERT; SaveChangesAsync executa a gravação no SQLite.
        context.Pacientes.Add(paciente);
        await context.SaveChangesAsync();
        return ServiceResult<PacienteReadDTO>.Ok(mapper.ToReadDTO(paciente));
    }

    public async Task<ServiceResult<PacienteReadDTO>> UpdateAsync(int id, PacienteUpdateDTO dto)
    {
        // FindAsync rastreia o paciente, necessário para que o PATCH seja salvo depois.
        var paciente = await context.Pacientes.FindAsync(id);
        if (paciente is null) return ServiceResult<PacienteReadDTO>.Fail(ServiceErrorType.NotFound, $"Paciente {id} não encontrado.");

        var errors = new List<string>();
        // CPF identifica o paciente e por regra de negócio não pode mudar após o cadastro.
        if (dto.Cpf is not null) errors.Add("CPF não pode ser alterado.");
        if (dto.Email is not null && !Validador.EmailValido(dto.Email)) errors.Add("Email em formato inválido.");
        if (dto.Telefone is not null && !Validador.TelefoneValido(dto.Telefone)) errors.Add("Telefone deve estar no formato (47) 98888-7777.");
        if (dto.DataNasc is not null && !Validador.DataNascValida(dto.DataNasc.Value)) errors.Add("Data de nascimento não pode ser no futuro.");
        if (dto.Nome is not null && string.IsNullOrWhiteSpace(dto.Nome)) errors.Add("Nome não pode ser vazio.");
        if (errors.Count > 0) return ServiceResult<PacienteReadDTO>.Fail(ServiceErrorType.Validation, errors.ToArray());

        // Copia somente campos não nulos do PATCH; campos ausentes preservam o valor atual.
        mapper.ApplyUpdate(paciente, dto);
        await context.SaveChangesAsync();
        return ServiceResult<PacienteReadDTO>.Ok(mapper.ToReadDTO(paciente));
    }

    public async Task<ServiceResult<bool>> DeleteAsync(int id)
    {
        var paciente = await context.Pacientes.FindAsync(id);
        if (paciente is null) return ServiceResult<bool>.Fail(ServiceErrorType.NotFound, $"Paciente {id} não encontrado.");
        // Impede exclusão quando há consulta futura vinculada a este paciente.
        if (await context.Consultas.AnyAsync(c => c.PacienteId == id && c.DataHora > DateTime.Now))
            return ServiceResult<bool>.Fail(ServiceErrorType.Conflict, "Não é possível remover: paciente possui consultas futuras.");

        // Remove agenda um DELETE; a remoção só é efetiva após SaveChangesAsync.
        context.Pacientes.Remove(paciente);
        await context.SaveChangesAsync();
        return ServiceResult<bool>.Ok(true);
    }

    private static List<string> Validar(PacienteCreateDTO dto)
    {
        var errors = new List<string>();
        if (!Validador.EmailValido(dto.Email)) errors.Add("Email em formato inválido.");
        if (!Validador.TelefoneValido(dto.Telefone)) errors.Add("Telefone deve estar no formato (47) 98888-7777.");
        if (dto.DataNasc is null || !Validador.DataNascValida(dto.DataNasc.Value)) errors.Add("Data de nascimento não pode ser no futuro.");
        if (!Validador.CpfValido(dto.Cpf)) errors.Add("CPF inválido.");
        return errors;
    }
}
