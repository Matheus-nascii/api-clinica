namespace ApiClinica.Models;

public class Consulta
{
    public int Id { get; set; }
    public int PacienteId { get; set; }
    public int MedicoId { get; set; }
    public DateTime DataHora { get; set; }

    // Navegação (chaves estrangeiras)
    public Paciente? Paciente { get; set; }
    public Medico? Medico { get; set; }

    // Regra do trabalho: cada consulta dura 30 minutos
    public const int DuracaoMinutos = 30;
}
