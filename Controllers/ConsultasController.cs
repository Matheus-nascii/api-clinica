using ApiClinica.DTOs;
using ApiClinica.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ApiClinica.Controllers;

[ApiController]
[Route("api/[controller]")]
// Exige JWT para leitura e criação de consultas.
[Authorize]
// Adaptador HTTP do recurso Consulta; regras de agendamento ficam no ConsultaService.
public class ConsultasController(IConsultaService service) : ControllerBase
{
    // GET lista consultas ordenadas por data com os nomes de paciente e médico.
    [HttpGet] public async Task<ActionResult<IEnumerable<ConsultaReadDTO>>> GetAll() => Ok(await service.GetAllAsync());
    [HttpGet("{id}")] public async Task<ActionResult<ConsultaReadDTO>> GetById(int id) => ToActionResult(await service.GetByIdAsync(id));
    [HttpPost]
    // POST agenda somente se paciente/médico existirem, a data for futura e não houver conflito.
    public async Task<ActionResult<ConsultaReadDTO>> Create(ConsultaCreateDTO dto)
    {
        var result = await service.CreateAsync(dto);
        return result.Success ? CreatedAtAction(nameof(GetById), new { id = result.Value!.Id }, result.Value) : ToActionResult(result);
    }
    // Reagendamento é exclusivo de Admin e reexecuta todas as validações de agenda.
    [HttpPatch("{id}"), Authorize(Roles = "Admin")]
    public async Task<ActionResult<ConsultaReadDTO>> Update(int id, ConsultaUpdateDTO dto) => ToActionResult(await service.UpdateAsync(id, dto));
    // Cancelamento da consulta é exclusivo de Admin.
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
