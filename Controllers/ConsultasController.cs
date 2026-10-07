using ApiClinica.Data;
using ApiClinica.DTOs;
using ApiClinica.Mappers;
using ApiClinica.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ApiClinica.Controllers;

[ApiController]
[Route("api/[controller]")] // -> /api/consultas
public class ConsultasController : ControllerBase
{
    private readonly AppDbContext _context;

    public ConsultasController(AppDbContext context)
    {
        _context = context;
    }

    // GET /api/consultas
    [HttpGet]
    public async Task<ActionResult<IEnumerable<ConsultaReadDTO>>> GetAll()
    {
        var consultas = await _context.Consultas
            .Include(c => c.Paciente)   // JOIN com Pacientes
            .Include(c => c.Medico)     // JOIN com Medicos
            .AsNoTracking()
            .OrderBy(c => c.DataHora)
            .ToListAsync();

        return Ok(consultas.Select(ConsultaMapper.ToReadDTO));
    }

    // GET /api/consultas/5
    [HttpGet("{id}")]
    public async Task<ActionResult<ConsultaReadDTO>> GetById(int id)
    {
        var consulta = await _context.Consultas
            .Include(c => c.Paciente)
            .Include(c => c.Medico)
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.Id == id);

        if (consulta == null)
            return NotFound(new { erro = $"Consulta {id} não encontrada." });

        return Ok(ConsultaMapper.ToReadDTO(consulta));
    }

    // POST /api/consultas
    [HttpPost]
    public async Task<ActionResult<ConsultaReadDTO>> Create(ConsultaCreateDTO dto)
    {
        var consulta = ConsultaMapper.ToEntity(dto);

        var erros = await ValidarConsulta(consulta);
        if (erros.Count > 0)
            return BadRequest(new { erros });

        _context.Consultas.Add(consulta);
        await _context.SaveChangesAsync();

        await CarregarNomes(consulta);
        return CreatedAtAction(nameof(GetById), new { id = consulta.Id }, ConsultaMapper.ToReadDTO(consulta));
    }

    // PATCH /api/consultas/5
    [HttpPatch("{id}")]
    public async Task<ActionResult<ConsultaReadDTO>> Update(int id, ConsultaUpdateDTO dto)
    {
        var consulta = await _context.Consultas.FindAsync(id);
        if (consulta == null)
            return NotFound(new { erro = $"Consulta {id} não encontrada." });

        // Aplica só o que veio preenchido e valida o RESULTADO final.
        // Assim, se trocar paciente ou médico, as regras de horário são revalidadas.
        ConsultaMapper.ApplyUpdate(consulta, dto);

        var erros = await ValidarConsulta(consulta);
        if (erros.Count > 0)
            return BadRequest(new { erros }); // nada é salvo

        await _context.SaveChangesAsync();

        await CarregarNomes(consulta);
        return Ok(ConsultaMapper.ToReadDTO(consulta));
    }

    // DELETE /api/consultas/5
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var consulta = await _context.Consultas.FindAsync(id);
        if (consulta == null)
            return NotFound(new { erro = $"Consulta {id} não encontrada." });

        _context.Consultas.Remove(consulta);
        await _context.SaveChangesAsync();

        return NoContent();
    }

    // ---------------- Regras de negócio da consulta ----------------
    private async Task<List<string>> ValidarConsulta(Consulta consulta)
    {
        var erros = new List<string>();

        // 1) Paciente existe?
        if (consulta.PacienteId <= 0 || !await _context.Pacientes.AnyAsync(p => p.Id == consulta.PacienteId))
            erros.Add($"Paciente {consulta.PacienteId} não existe.");

        // 2) Médico existe?
        if (consulta.MedicoId <= 0 || !await _context.Medicos.AnyAsync(m => m.Id == consulta.MedicoId))
            erros.Add($"Médico {consulta.MedicoId} não existe.");

        // 3) Não pode ser no passado
        if (consulta.DataHora < DateTime.Now)
            erros.Add("Não é possível agendar consulta no passado.");

        if (erros.Count > 0)
            return erros;

        // 4) Horário igual ou sobreposição (cada consulta dura 30 min).
        // Duas consultas se sobrepõem quando a diferença entre os inícios é menor que 30 min.
        // Ex.: 10:00 e 10:15 -> sobrepõe | 10:00 e 10:30 -> OK
        var limiteInicio = consulta.DataHora.AddMinutes(-Consulta.DuracaoMinutos);
        var limiteFim = consulta.DataHora.AddMinutes(Consulta.DuracaoMinutos);

        var conflitoMedico = await _context.Consultas
            .Where(c => c.Id != consulta.Id                // ignora ela mesma no PATCH
                     && c.MedicoId == consulta.MedicoId
                     && c.DataHora > limiteInicio
                     && c.DataHora < limiteFim)
            .FirstOrDefaultAsync();

        if (conflitoMedico != null)
            erros.Add(conflitoMedico.DataHora == consulta.DataHora
                ? "O médico já possui uma consulta neste mesmo horário."
                : $"Sobreposição de horário: o médico já tem consulta às {conflitoMedico.DataHora:HH:mm}.");

        var conflitoPaciente = await _context.Consultas
            .Where(c => c.Id != consulta.Id
                     && c.PacienteId == consulta.PacienteId
                     && c.DataHora > limiteInicio
                     && c.DataHora < limiteFim)
            .FirstOrDefaultAsync();

        if (conflitoPaciente != null)
            erros.Add(conflitoPaciente.DataHora == consulta.DataHora
                ? "O paciente já possui uma consulta neste mesmo horário."
                : $"Sobreposição de horário: o paciente já tem consulta às {conflitoPaciente.DataHora:HH:mm}.");

        return erros;
    }

    // Carrega Paciente e Médico para preencher os nomes no DTO de retorno
    private async Task CarregarNomes(Consulta consulta)
    {
        await _context.Entry(consulta).Reference(c => c.Paciente).LoadAsync();
        await _context.Entry(consulta).Reference(c => c.Medico).LoadAsync();
    }
}
