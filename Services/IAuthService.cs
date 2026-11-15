using ApiClinica.DTOs;

namespace ApiClinica.Services;

// Contrato dos casos de uso de autenticacao e inicializacao controlada do usuario Admin.
public interface IAuthService
{
    Task<ServiceResult<AuthResponseDTO>> RegisterAsync(RegisterDTO dto);
    Task<ServiceResult<AuthResponseDTO>> LoginAsync(LoginDTO dto);
    Task InitializeAdminAsync();
}
