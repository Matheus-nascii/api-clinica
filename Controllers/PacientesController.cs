using ApiClinica.DTOs;
using ApiClinica.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ApiClinica.Controllers;

[ApiController]
[Route("api/[controller]")]
// Todas as rotas de pacientes exigem JWT válido; sem ele o middleware responde 401 antes da ação.
[Authorize]
// Adaptador HTTP do recurso Paciente: delega os casos de uso ao service injetado.
public class PacientesController(IPacienteService service) : ControllerBase
{
    // GET consulta todos os pacientes e retorna somente DTOs de leitura.
    [HttpGet] public async Task<ActionResult<IEnumerable<PacienteReadDTO>>> GetAll() => Ok(await service.GetAllAsync());

    // GET por ID delega a busca; ID ausente é convertido em 404.
    [HttpGet("{id}")] public async Task<ActionResult<PacienteReadDTO>> GetById(int id) => ToActionResult(await service.GetByIdAsync(id));

    [HttpPost]
    // POST aceita usuário autenticado, valida e cria o paciente; sucesso retorna 201 com Location.
    public async Task<ActionResult<PacienteReadDTO>> Create(PacienteCreateDTO dto)
    {
        var result = await service.CreateAsync(dto);
        return result.Success ? CreatedAtAction(nameof(GetById), new { id = result.Value!.Id }, result.Value) : ToActionResult(result);
    }

    // PATCH exige claim Role=Admin; usuário comum recebe 403 sem executar o service.
    [HttpPatch("{id}"), Authorize(Roles = "Admin")]
    public async Task<ActionResult<PacienteReadDTO>> Update(int id, PacienteUpdateDTO dto) => ToActionResult(await service.UpdateAsync(id, dto));

    // DELETE também é exclusivo de Admin e só remove se não houver consulta futura.
    [HttpDelete("{id}"), Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(int id)
    {
        var result = await service.DeleteAsync(id);
        return result.Success ? NoContent() : ToErrorResult(result);
    }

    // Padroniza respostas de leitura/alteração e preserva a lista de erros produzida pela regra de negócio.
    private ActionResult<T> ToActionResult<T>(ServiceResult<T> result) => result.Success ? Ok(result.Value) : new ObjectResult(new { erros = result.Errors }) { StatusCode = Status(result.ErrorType) };
    private IActionResult ToErrorResult<T>(ServiceResult<T> result) => new ObjectResult(new { erros = result.Errors }) { StatusCode = Status(result.ErrorType) };
    private static int Status(ServiceErrorType type) => type == ServiceErrorType.NotFound ? StatusCodes.Status404NotFound : type == ServiceErrorType.Conflict ? StatusCodes.Status409Conflict : StatusCodes.Status400BadRequest;
}
