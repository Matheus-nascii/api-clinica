using ApiClinica.DTOs;

namespace ApiClinica.Services;

// Contrato dos casos de uso de paciente consumido pelo controller via injecao de dependencia.
public interface IPacienteService
{
    Task<IEnumerable<PacienteReadDTO>> GetAllAsync();
    Task<ServiceResult<PacienteReadDTO>> GetByIdAsync(int id);
    Task<ServiceResult<PacienteReadDTO>> CreateAsync(PacienteCreateDTO dto);
    Task<ServiceResult<PacienteReadDTO>> UpdateAsync(int id, PacienteUpdateDTO dto);
    Task<ServiceResult<bool>> DeleteAsync(int id);
}
