using ApiClinica.DTOs;
using ApiClinica.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ApiClinica.Controllers;

[ApiController]
[Route("api/[controller]")]
// O JWT é obrigatório em todas as operações com médicos.
[Authorize]
// Adaptador HTTP do recurso Medico: converte os resultados do service em respostas HTTP.
public class MedicosController(IMedicoService service) : ControllerBase
{
    // GET lista médicos autenticados usando DTOs, sem expor a entidade do EF.
    [HttpGet] public async Task<ActionResult<IEnumerable<MedicoReadDTO>>> GetAll() => Ok(await service.GetAllAsync());
    [HttpGet("{id}")] public async Task<ActionResult<MedicoReadDTO>> GetById(int id) => ToActionResult(await service.GetByIdAsync(id));
    [HttpPost]
    // POST cria médico após validar email e telefone.
    public async Task<ActionResult<MedicoReadDTO>> Create(MedicoCreateDTO dto)
    {
        var result = await service.CreateAsync(dto);
        return result.Success ? CreatedAtAction(nameof(GetById), new { id = result.Value!.Id }, result.Value) : ToActionResult(result);
    }
    // Somente Admin pode alterar dados de médico.
    [HttpPatch("{id}"), Authorize(Roles = "Admin")]
    public async Task<ActionResult<MedicoReadDTO>> Update(int id, MedicoUpdateDTO dto) => ToActionResult(await service.UpdateAsync(id, dto));
    // Somente Admin pode excluir; consultas futuras bloqueiam a operação no service.
    [HttpDelete("{id}"), Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(int id)
    {
        var result = await service.DeleteAsync(id);
        return result.Success ? NoContent() : ToErrorResult(result);
    }
    private ActionResult<T> ToActionResult<T>(ServiceResult<T> result) => result.Success ? Ok(result.Value) : new ObjectResult(new { erros = result.Errors }) { StatusCode = Status(result.ErrorType) };
    private IActionResult ToErrorResult<T>(ServiceResult<T> result) => new ObjectResult(new { erros = result.Errors }) { StatusCode = Status(result.ErrorType) };
    private static int Status(ServiceErrorType type) => type == ServiceErrorType.NotFound ? 404 : type == ServiceErrorType.Conflict ? 409 : 400;
}
