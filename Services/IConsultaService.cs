using ApiClinica.DTOs;

namespace ApiClinica.Services;

// Contrato dos casos de uso de consulta consumido pelo controller via injecao de dependencia.
public interface IConsultaService
{
    Task<IEnumerable<ConsultaReadDTO>> GetAllAsync();
    Task<ServiceResult<ConsultaReadDTO>> GetByIdAsync(int id);
    Task<ServiceResult<ConsultaReadDTO>> CreateAsync(ConsultaCreateDTO dto);
    Task<ServiceResult<ConsultaReadDTO>> UpdateAsync(int id, ConsultaUpdateDTO dto);
    Task<ServiceResult<bool>> DeleteAsync(int id);
}
