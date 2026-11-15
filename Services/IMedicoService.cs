using ApiClinica.DTOs;

namespace ApiClinica.Services;

// Contrato dos casos de uso de medico consumido pelo controller via injecao de dependencia.
public interface IMedicoService
{
    Task<IEnumerable<MedicoReadDTO>> GetAllAsync();
    Task<ServiceResult<MedicoReadDTO>> GetByIdAsync(int id);
    Task<ServiceResult<MedicoReadDTO>> CreateAsync(MedicoCreateDTO dto);
    Task<ServiceResult<MedicoReadDTO>> UpdateAsync(int id, MedicoUpdateDTO dto);
    Task<ServiceResult<bool>> DeleteAsync(int id);
}
