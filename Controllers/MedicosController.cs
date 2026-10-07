using ApiClinica.Data;
using ApiClinica.DTOs;
using ApiClinica.Mappers;
using ApiClinica.Validations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ApiClinica.Controllers;

[ApiController]
[Route("api/[controller]")] // -> /api/medicos
public class MedicosController : ControllerBase
{
    private readonly AppDbContext _context;

    public MedicosController(AppDbContext context)
    {
        _context = context;
    }

    // GET /api/medicos
    [HttpGet]
    public async Task<ActionResult<IEnumerable<MedicoReadDTO>>> GetAll()
    {
        var medicos = await _context.Medicos.AsNoTracking().ToListAsync();
        return Ok(medicos.Select(MedicoMapper.ToReadDTO));
    }

    // GET /api/medicos/5
    [HttpGet("{id}")]
    public async Task<ActionResult<MedicoReadDTO>> GetById(int id)
    {
        var medico = await _context.Medicos.FindAsync(id);
        if (medico == null)
            return NotFound(new { erro = $"Médico {id} não encontrado." });

        return Ok(MedicoMapper.ToReadDTO(medico));
    }

    // POST /api/medicos
    [HttpPost]
    public async Task<ActionResult<MedicoReadDTO>> Create(MedicoCreateDTO dto)
    {
        var erros = new List<string>();

        if (!Validador.EmailValido(dto.Email))
            erros.Add("Email em formato inválido.");
        if (!Validador.TelefoneValido(dto.Telefone))
            erros.Add("Telefone deve estar no formato (47) 98888-7777.");

        if (erros.Count > 0)
            return BadRequest(new { erros });

        var medico = MedicoMapper.ToEntity(dto);
        _context.Medicos.Add(medico);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetById), new { id = medico.Id }, MedicoMapper.ToReadDTO(medico));
    }

    // PATCH /api/medicos/5
    [HttpPatch("{id}")]
    public async Task<ActionResult<MedicoReadDTO>> Update(int id, MedicoUpdateDTO dto)
    {
        var medico = await _context.Medicos.FindAsync(id);
        if (medico == null)
            return NotFound(new { erro = $"Médico {id} não encontrado." });

        var erros = new List<string>();

        if (dto.Email != null && !Validador.EmailValido(dto.Email))
            erros.Add("Email em formato inválido.");
        if (dto.Telefone != null && !Validador.TelefoneValido(dto.Telefone))
            erros.Add("Telefone deve estar no formato (47) 98888-7777.");
        if (dto.Nome != null && string.IsNullOrWhiteSpace(dto.Nome))
            erros.Add("Nome não pode ser vazio.");

        if (erros.Count > 0)
            return BadRequest(new { erros });

        MedicoMapper.ApplyUpdate(medico, dto);
        await _context.SaveChangesAsync();

        return Ok(MedicoMapper.ToReadDTO(medico));
    }

    // DELETE /api/medicos/5
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var medico = await _context.Medicos.FindAsync(id);
        if (medico == null)
            return NotFound(new { erro = $"Médico {id} não encontrado." });

        var agora = DateTime.Now;
        if (await _context.Consultas.AnyAsync(c => c.MedicoId == id && c.DataHora > agora))
            return Conflict(new { erro = "Não é possível remover: médico possui consultas futuras." });

        _context.Medicos.Remove(medico);
        await _context.SaveChangesAsync();

        return NoContent();
    }
}
