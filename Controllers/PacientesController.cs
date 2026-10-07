using ApiClinica.Data;
using ApiClinica.DTOs;
using ApiClinica.Mappers;
using ApiClinica.Validations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ApiClinica.Controllers;

[ApiController]
[Route("api/[controller]")] // -> /api/pacientes
public class PacientesController : ControllerBase
{
    private readonly AppDbContext _context;

    // O DbContext é injetado pelo ASP.NET (injeção de dependência)
    public PacientesController(AppDbContext context)
    {
        _context = context;
    }

    // GET /api/pacientes
    [HttpGet]
    public async Task<ActionResult<IEnumerable<PacienteReadDTO>>> GetAll()
    {
        var pacientes = await _context.Pacientes.AsNoTracking().ToListAsync();
        return Ok(pacientes.Select(PacienteMapper.ToReadDTO));
    }

    // GET /api/pacientes/5
    [HttpGet("{id}")]
    public async Task<ActionResult<PacienteReadDTO>> GetById(int id)
    {
        var paciente = await _context.Pacientes.FindAsync(id);
        if (paciente == null)
            return NotFound(new { erro = $"Paciente {id} não encontrado." });

        return Ok(PacienteMapper.ToReadDTO(paciente));
    }

    // POST /api/pacientes
    [HttpPost]
    public async Task<ActionResult<PacienteReadDTO>> Create(PacienteCreateDTO dto)
    {
        var erros = new List<string>();

        if (!Validador.EmailValido(dto.Email))
            erros.Add("Email em formato inválido.");
        if (!Validador.TelefoneValido(dto.Telefone))
            erros.Add("Telefone deve estar no formato (47) 98888-7777.");
        if (dto.DataNasc == null || !Validador.DataNascValida(dto.DataNasc.Value))
            erros.Add("Data de nascimento não pode ser no futuro.");
        if (!Validador.CpfValido(dto.Cpf))
            erros.Add("CPF inválido.");

        if (erros.Count > 0)
            return BadRequest(new { erros });

        // CPF duplicado?
        var cpf = Validador.SomenteNumeros(dto.Cpf);
        if (await _context.Pacientes.AnyAsync(p => p.Cpf == cpf))
            return Conflict(new { erro = "Já existe um paciente cadastrado com este CPF." });

        var paciente = PacienteMapper.ToEntity(dto);
        _context.Pacientes.Add(paciente);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetById), new { id = paciente.Id }, PacienteMapper.ToReadDTO(paciente));
    }

    // PATCH /api/pacientes/5
    [HttpPatch("{id}")]
    public async Task<ActionResult<PacienteReadDTO>> Update(int id, PacienteUpdateDTO dto)
    {
        var paciente = await _context.Pacientes.FindAsync(id);
        if (paciente == null)
            return NotFound(new { erro = $"Paciente {id} não encontrado." });

        var erros = new List<string>();

        // Só valida os campos que foram enviados (null = não alterar)
        if (dto.Cpf != null)
            erros.Add("CPF não pode ser alterado.");
        if (dto.Email != null && !Validador.EmailValido(dto.Email))
            erros.Add("Email em formato inválido.");
        if (dto.Telefone != null && !Validador.TelefoneValido(dto.Telefone))
            erros.Add("Telefone deve estar no formato (47) 98888-7777.");
        if (dto.DataNasc != null && !Validador.DataNascValida(dto.DataNasc.Value))
            erros.Add("Data de nascimento não pode ser no futuro.");
        if (dto.Nome != null && string.IsNullOrWhiteSpace(dto.Nome))
            erros.Add("Nome não pode ser vazio.");

        if (erros.Count > 0)
            return BadRequest(new { erros });

        PacienteMapper.ApplyUpdate(paciente, dto);
        await _context.SaveChangesAsync();

        return Ok(PacienteMapper.ToReadDTO(paciente));
    }

    // DELETE /api/pacientes/5
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var paciente = await _context.Pacientes.FindAsync(id);
        if (paciente == null)
            return NotFound(new { erro = $"Paciente {id} não encontrado." });

        var agora = DateTime.Now;
        if (await _context.Consultas.AnyAsync(c => c.PacienteId == id && c.DataHora > agora))
            return Conflict(new { erro = "Não é possível remover: paciente possui consultas futuras." });

        _context.Pacientes.Remove(paciente);
        await _context.SaveChangesAsync();

        return NoContent();
    }
}
