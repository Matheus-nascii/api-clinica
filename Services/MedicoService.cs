using ApiClinica.Data;
using ApiClinica.DTOs;
using ApiClinica.Interfaces;
using ApiClinica.Validations;
using Microsoft.EntityFrameworkCore;

namespace ApiClinica.Services;

// Implementa os casos de uso e as regras de negocio do medico.
public class MedicoService(AppDbContext context, IMedicoMapper mapper) : IMedicoService
{
    public async Task<IEnumerable<MedicoReadDTO>> GetAllAsync() =>
        // A listagem não será editada nesta requisição, por isso não precisa de rastreamento do EF.
        (await context.Medicos.AsNoTracking().ToListAsync()).Select(mapper.ToReadDTO);

    public async Task<ServiceResult<MedicoReadDTO>> GetByIdAsync(int id)
    {
        var medico = await context.Medicos.AsNoTracking().FirstOrDefaultAsync(m => m.Id == id);
        return medico is null ? ServiceResult<MedicoReadDTO>.Fail(ServiceErrorType.NotFound, $"Médico {id} não encontrado.") : ServiceResult<MedicoReadDTO>.Ok(mapper.ToReadDTO(medico));
    }

    public async Task<ServiceResult<MedicoReadDTO>> CreateAsync(MedicoCreateDTO dto)
    {
        // Email e telefone são conferidos antes de o médico ser incluído na tabela Medicos.
        var errors = Validar(dto);
        if (errors.Count > 0) return ServiceResult<MedicoReadDTO>.Fail(ServiceErrorType.Validation, errors.ToArray());
        var medico = mapper.ToEntity(dto);
        // INSERT no SQLite ocorre somente ao confirmar SaveChangesAsync.
        context.Medicos.Add(medico);
        await context.SaveChangesAsync();
        return ServiceResult<MedicoReadDTO>.Ok(mapper.ToReadDTO(medico));
    }

    public async Task<ServiceResult<MedicoReadDTO>> UpdateAsync(int id, MedicoUpdateDTO dto)
    {
        // Carrega a entidade rastreada para que as alterações parciais sejam persistidas.
        var medico = await context.Medicos.FindAsync(id);
        if (medico is null) return ServiceResult<MedicoReadDTO>.Fail(ServiceErrorType.NotFound, $"Médico {id} não encontrado.");
        var errors = new List<string>();
        if (dto.Email is not null && !Validador.EmailValido(dto.Email)) errors.Add("Email em formato inválido.");
        if (dto.Telefone is not null && !Validador.TelefoneValido(dto.Telefone)) errors.Add("Telefone deve estar no formato (47) 98888-7777.");
        if (dto.Nome is not null && string.IsNullOrWhiteSpace(dto.Nome)) errors.Add("Nome não pode ser vazio.");
        if (errors.Count > 0) return ServiceResult<MedicoReadDTO>.Fail(ServiceErrorType.Validation, errors.ToArray());
        // Atualiza apenas propriedades enviadas no PATCH; null significa manter o valor salvo.
        mapper.ApplyUpdate(medico, dto);
        await context.SaveChangesAsync();
        return ServiceResult<MedicoReadDTO>.Ok(mapper.ToReadDTO(medico));
    }

    public async Task<ServiceResult<bool>> DeleteAsync(int id)
    {
        var medico = await context.Medicos.FindAsync(id);
        if (medico is null) return ServiceResult<bool>.Fail(ServiceErrorType.NotFound, $"Médico {id} não encontrado.");
        // Não permite apagar médico que ainda possui atendimento futuro agendado.
        if (await context.Consultas.AnyAsync(c => c.MedicoId == id && c.DataHora > DateTime.Now)) return ServiceResult<bool>.Fail(ServiceErrorType.Conflict, "Não é possível remover: médico possui consultas futuras.");
        context.Medicos.Remove(medico);
        await context.SaveChangesAsync();
        return ServiceResult<bool>.Ok(true);
    }

    private static List<string> Validar(MedicoCreateDTO dto)
    {
        var errors = new List<string>();
        if (!Validador.EmailValido(dto.Email)) errors.Add("Email em formato inválido.");
        if (!Validador.TelefoneValido(dto.Telefone)) errors.Add("Telefone deve estar no formato (47) 98888-7777.");
        return errors;
    }
}
