using ApiClinica.DTOs;
using ApiClinica.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ApiClinica.Controllers;

[ApiController]
[Route("api/[controller]")]
// Cadastro e login precisam ser públicos para que o cliente obtenha seu primeiro token.
[AllowAnonymous]
// Endpoints publicos que criam usuarios ou emitem JWT apos delegar a validacao ao AuthService.
public class AuthController(IAuthService service) : ControllerBase
{
    [HttpPost("register")]
    // Recebe nome, email e senha; o service cria Usuario e devolve JWT, nunca o hash da senha.
    public async Task<ActionResult<AuthResponseDTO>> Register(RegisterDTO dto) => ToActionResult(await service.RegisterAsync(dto));

    [HttpPost("login")]
    // Confere as credenciais no service e devolve token somente se o hash for validado.
    public async Task<ActionResult<AuthResponseDTO>> Login(LoginDTO dto) => ToActionResult(await service.LoginAsync(dto));

    // Traduz o resultado do service para HTTP: conflito=409, credencial inválida=401 e validação=400.
    private ActionResult<AuthResponseDTO> ToActionResult(ServiceResult<AuthResponseDTO> result) => result.Success
        ? Ok(result.Value)
        : new ObjectResult(new { erros = result.Errors }) { StatusCode = result.ErrorType == ServiceErrorType.Conflict ? 409 : result.ErrorType == ServiceErrorType.Unauthorized ? 401 : 400 };
}
